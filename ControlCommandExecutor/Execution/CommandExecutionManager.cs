using ControlCommandAnalyser.Model;
using ControlCommandExecutor.Executors;
using ControlCommandExecutor.Executors.Interface;
using DTO.Base.Models;
using DTO.Service;
using Errors.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Utilities.TextEditor;
using static Utilities.LoggerUtility;

namespace ControlCommandExecutor.Execution
{
  /// <summary>
  /// Основной исполнитель команд контроля.
  /// </summary>
  public class CommandExecutionManager
  {
    private readonly Dictionary<string, ICommandExecutor> _executors = new();
    private readonly IUserMessageService _console;
    private readonly ITextEditorAdapter translationControl;
    private readonly string? _opkFilePath;

    private ProtocolModel protocolModel = new ProtocolModel();

    /// <summary>
    /// Событие, которое вызывается при добавлении ошибки.
    /// </summary>
    public event Action<ErrorItem>? AddError;

    /// <summary>
    /// Событие, которое вызывается при очистке ошибок.
    /// </summary>
    public event Action? ClearError;

    /// <summary>
    /// Команды программы контроля.
    /// </summary>
    public List<BaseCommandModel> CommandsToExecute { get; set; } = new();

    /// <summary>Набор строк (в ПРАВОМ редакторе), на которых установлены точки останова.</summary>
    private readonly IReadOnlyCollection<int> _breakpoints;

    /// <summary>TCS для ожидания продолжения выполнения после остановки на breakpoint.</summary>
    private TaskCompletionSource<bool>? _breakpointTcs;

    public void ClearErrorsMethod() => ClearError?.Invoke();

    public void AddErrorMethod(ErrorItem errorItem) => AddError?.Invoke(errorItem);

    /// <summary>
    /// Создаёт менеджер выполнения команд.
    /// </summary>
    public CommandExecutionManager(
      IUserMessageService console,
      ITextEditorAdapter textEditor,
      List<BaseCommandModel> controlProgram,
      string? opkFilePath,
      IReadOnlyCollection<int>? breakpoints = null)
    {
      _console = console;
      translationControl = textEditor;
      CommandsToExecute = controlProgram;
      _opkFilePath = opkFilePath;
      _breakpoints = breakpoints ?? Array.Empty<int>();

      LogInformation($"[CEM] ctor Breakpoints: {string.Join(", ", _breakpoints)}");

      RegisterExecutors();
    }

    /// <summary>
    /// Подсветить и прокрутить к строке в редакторе, если адаптер это умеет.
    /// Имеем дело с форматированным текстом (правый редактор).
    /// </summary>
    private void HighlightLineInEditor(int line)
    {
      if (line <= 0 || translationControl == null)
        return;

      void Inner()
      {
        var type = translationControl.GetType();

        try
        {
          var setActive = type.GetMethod("SetActiveLine", new[] { typeof(int) });
          setActive?.Invoke(translationControl, new object[] { line });
        }
        catch { }

        try
        {
          var scroll = type.GetMethod("ScrollToLine", new[] { typeof(int) });
          scroll?.Invoke(translationControl, new object[] { line });
        }
        catch { }
      }

      // Пытаемся использовать Dispatcher конкретного контрола
      if (translationControl is DispatcherObject dObj)
      {
        if (dObj.Dispatcher.CheckAccess())
        {
          Inner();
        }
        else
        {
          dObj.Dispatcher.BeginInvoke((Action)Inner);
        }
      }
      else
      {
        // Fallback — через глобальный Application.Dispatcher
        var appDisp = Application.Current?.Dispatcher;
        if (appDisp != null && !appDisp.CheckAccess())
        {
          appDisp.BeginInvoke((Action)Inner);
        }
        else
        {
          Inner();
        }
      }
    }

    /// <summary>
    /// Для команды с индексом commandIndex находим, есть ли breakpoint
    /// в её диапазоне строк форматированного текста.
    /// Диапазон: [FormattedStartLineNumber(current) ; FormattedStartLineNumber(next) - 1).
    /// Возвращает строку breakpoint'а или null, если для этой команды нет точек.
    /// </summary>
    private IEnumerable<int> GetBreakpointHitsForCommand(int commandIndex)
    {
      if (_breakpoints == null || _breakpoints.Count == 0)
        yield break;

      if (commandIndex < 0 || commandIndex >= CommandsToExecute.Count)
        yield break;

      var current = CommandsToExecute[commandIndex];
      int start = current.FormattedStartLineNumber;

      if (start <= 0)
      {
        LogDebug($"[CEM] Cmd {current.CommandNumber} has FormattedStartLineNumber = {start}, пропускаем breakpoints.");
        yield break;
      }

      int endExclusive;

      if (commandIndex + 1 < CommandsToExecute.Count)
      {
        var next = CommandsToExecute[commandIndex + 1];
        endExclusive = next.FormattedStartLineNumber > 0
          ? next.FormattedStartLineNumber
          : int.MaxValue;
      }
      else
      {
        endExclusive = int.MaxValue;
      }

      var hits = _breakpoints
        .Where(line => line >= start && line < endExclusive)
        .OrderBy(line => line)
        .ToList();

      if (hits.Count == 0)
        yield break;

      LogInformation($"[CEM] Command {current.CommandNumber} [{start}; {endExclusive}) имеет breakpoints: {string.Join(", ", hits)}");

      foreach (var h in hits)
        yield return h;
    }

    /// <summary>
    /// Проверяет, есть ли точка остановки на указанной строке форматированного текста,
    /// и при наличии — подсветить строку и подождать продолжения.
    /// Вызывать из executors для внутренних под-операций команд.
    /// </summary>
    public async Task HitBreakpointIfNeededAsync(int formattedLine, CancellationToken cancellationToken)
    {
      if (formattedLine <= 0 || _breakpoints == null || _breakpoints.Count == 0)
        return;

      if (!_breakpoints.Contains(formattedLine))
        return;

      LogInformation($"[CEM] Внутренний breakpoint на строке {formattedLine}");
      HighlightLineInEditor(formattedLine);

      await WaitOnBreakpointAsync(cancellationToken);
    }


    /// <summary>
    /// Выполняет все команды по очереди, с поддержкой точек останова.
    /// </summary>
    public async Task ExecuteAllAsync(CancellationToken cancellationToken = default)
    {
      int i = 0;
      while (i < CommandsToExecute.Count)
      {
        cancellationToken.ThrowIfCancellationRequested();

        var command = CommandsToExecute[i];

        foreach (var hitLine in GetBreakpointHitsForCommand(i))
        {
          LogInformation($"[CEM] Пауза на breakpoint: line {hitLine}, cmd {CommandsToExecute[i].CommandNumber}");
          HighlightLineInEditor(hitLine);
          await WaitOnBreakpointAsync(cancellationToken);
        }

        var context = new CommandExecutionContext(this, command, _console, translationControl, _opkFilePath, cancellationToken)
        {
          JumpToCommandNumber = (number) =>
          {
            int newIndex = CommandsToExecute.FindIndex(cmd => cmd.CommandNumber == number);
            if (newIndex >= 0)
            {
              i = newIndex - 1;
            }
          }
        };

        if (_executors.TryGetValue(command.Mnemonic, out var executor))
        {
          await executor.ExecuteAsync(context, protocolModel);
        }
        else
        {
          await _console.ShowMessageAsync(new ShowMessageModel(
            "Неизвестная команда",
            message: command.Mnemonic,
            type: ShowMessageModel.MessageType.Error));
        }

        i++;
      }
    }

    /// <summary>Ожидание продолжения после остановки на breakpoint.</summary>
    private Task WaitOnBreakpointAsync(CancellationToken cancellationToken)
    {
      var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
      _breakpointTcs = tcs;

      if (cancellationToken.CanBeCanceled)
      {
        cancellationToken.Register(() =>
        {
          tcs.TrySetCanceled(cancellationToken);
        });
      }

      // Автоматическое продолжение через 3 секунды
      _ = Task.Run(async () =>
      {
        try
        {
          await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
          tcs.TrySetResult(true); // если уже нажали "Продолжить" — тихо проигнорируется
        }
        catch (TaskCanceledException)
        {
          // игнорируем, если выполнение отменили раньше
        }
      }, cancellationToken);

      return tcs.Task;
    }


    /// <summary>Продолжить выполнение после breakpoint (вызывается из UI).</summary>
    public void ContinueFromBreakpoint()
    {
      _breakpointTcs?.TrySetResult(true);
    }

    /// <summary>
    /// Выполнить одну команду по предоставленной модели
    /// (без учёта breakpoints; "шаг" можно делать через ExecuteAllAsync на подсписке).
    /// </summary>
    public async Task ExecuteOneAsync(BaseCommandModel command)
    {
      if (_executors.TryGetValue(command.Mnemonic, out var executor))
      {
        var context = new CommandExecutionContext(this, command, _console, translationControl, _opkFilePath);
        await executor.ExecuteAsync(context, protocolModel);
      }
      else
      {
        await _console.ShowMessageAsync(new ShowMessageModel(
          "Неизвестная команда",
          message: command.Mnemonic,
          type: ShowMessageModel.MessageType.Error));
      }
    }

    /// <summary>
    /// Регистрирует исполнителей команд.
    /// </summary>
    private void RegisterExecutors()
    {
      var executorInterface = typeof(ICommandExecutor);
      var executorTypes = Assembly.GetExecutingAssembly()
          .GetTypes()
          .Where(t => !t.IsAbstract && !t.IsInterface && executorInterface.IsAssignableFrom(t));

      foreach (var type in executorTypes)
      {
        var instance = (ICommandExecutor)Activator.CreateInstance(type)!;
        _executors[instance.Mnemonic] = instance;
      }
    }

    /// <summary>
    /// Пропускает команды до указанного номера и продолжает выполнение с неё (без учёта breakpoints).
    /// </summary>
    public async Task JumpToCommandAndExecuteAsync(string commandNumber)
    {
      int index = CommandsToExecute.FindIndex(cmd => cmd.CommandNumber == commandNumber);

      if (index < 0)
      {
        await _console.ShowMessageAsync(new ShowMessageModel(
            $"Команда с номером {commandNumber} не найдена.",
            message: "",
            type: ShowMessageModel.MessageType.Error));
        return;
      }

      for (int i = index; i < CommandsToExecute.Count; i++)
      {
        await ExecuteOneAsync(CommandsToExecute[i]);
      }
    }
  }
}