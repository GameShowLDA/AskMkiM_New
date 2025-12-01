using ControlCommandAnalyser.Model;
using ControlCommandExecutor.Executors.Interface;
using DTO.Base.Models;
using DTO.Service;
using Errors.Models;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Utilities;
using Utilities.TextEditor;

namespace ControlCommandExecutor.Execution
{
  /// <summary>
  /// Основной исполнитель команд контроля.
  /// </summary>
  public class CommandExecutionManager
  {
    private readonly Dictionary<string, ICommandExecutor> _executors = new();
    private readonly IUserMessageService _console;
    private readonly ITextEditorAdapter _translationControl;
    private readonly ProtocolModel _protocolModel = new();
    private readonly string? _opkFilePath;

    /// <summary>
    /// Команды программы контроля.
    /// </summary>
    public List<BaseCommandModel> CommandsToExecute { get; set; } = new();

    /// <summary>
    /// Сырой список точек останова (как пришёл из редактора).
    /// </summary>
    private readonly IReadOnlyCollection<int> _rawBreakpoints;

    /// <summary>
    /// Набор строк-заголовков команд (FormattedStartLineNumber), на которых реально должны останавливаться.
    /// </summary>
    private readonly HashSet<int> _headerBreakpoints;

    /// <summary>
    /// TCS для ожидания продолжения выполнения после остановки на breakpoint.
    /// </summary>
    private TaskCompletionSource<bool>? _breakpointTcs;

    /// <summary>
    /// Событие, которое вызывается при добавлении ошибки.
    /// </summary>
    public event Action<ErrorItem>? AddError;

    /// <summary>
    /// Событие, которое вызывается при очистке ошибок.
    /// </summary>
    public event Action? ClearError;

    public void ClearErrorsMethod() => ClearError?.Invoke();

    public void AddErrorMethod(ErrorItem errorItem) => AddError?.Invoke(errorItem);

    /// <summary>
    /// Создаёт менеджер выполнения команд.
    /// </summary>
    /// <param name="console">Сервис вывода сообщений.</param>
    /// <param name="textEditor">Адаптер текстового редактора (правый, с трансляцией).</param>
    /// <param name="controlProgram">Список команд программы контроля.</param>
    /// <param name="opkFilePath">Путь к исходному OPK-файлу.</param>
    /// <param name="breakpoints">
    /// Строки, на которых пользователь поставил точки останова (по форматированному тексту).
    /// </param>
    public CommandExecutionManager(
      IUserMessageService console,
      ITextEditorAdapter textEditor,
      List<BaseCommandModel> controlProgram,
      string? opkFilePath,
      IReadOnlyCollection<int>? breakpoints = null)
    {
      _console = console;
      _translationControl = textEditor;
      CommandsToExecute = controlProgram;
      _opkFilePath = opkFilePath;

      _rawBreakpoints = breakpoints ?? Array.Empty<int>();

      _headerBreakpoints = BuildHeaderBreakpoints(CommandsToExecute, _rawBreakpoints);

      LoggerUtility.LogInformation($"[CEM] ctor raw breakpoints      : {string.Join(", ", _rawBreakpoints)}");
      LoggerUtility.LogInformation($"[CEM] ctor header breakpoints   : {string.Join(", ", _headerBreakpoints)}");

      RegisterExecutors();
    }

    /// <summary>
    /// Строим множество точек останова по заголовкам команд.
    /// Любая точка внутри тела команды → её FormattedStartLineNumber.
    /// </summary>
    private static HashSet<int> BuildHeaderBreakpoints(
      List<BaseCommandModel> commands,
      IReadOnlyCollection<int> rawBreakpoints)
    {
      var result = new HashSet<int>();

      if (commands == null || commands.Count == 0 || rawBreakpoints == null || rawBreakpoints.Count == 0)
        return result;

      var ordered = commands
        .Where(c => c != null && c.FormattedStartLineNumber > 0)
        .OrderBy(c => c.FormattedStartLineNumber)
        .ToList();

      if (ordered.Count == 0)
        return result;

      foreach (var bp in rawBreakpoints)
      {
        var cmd = ordered.LastOrDefault(c => c.FormattedStartLineNumber <= bp);
        if (cmd != null)
        {
          result.Add(cmd.FormattedStartLineNumber);
          LoggerUtility.LogInformation(
            $"[CEM] breakpoint line {bp} → cmd {cmd.CommandNumber} (header {cmd.FormattedStartLineNumber})");
        }
        else
        {
          LoggerUtility.LogWarning($"[CEM] breakpoint line {bp} не попал ни в одну команду");
        }
      }

      return result;
    }

    /// <summary>
    /// Подсветить и прокрутить к строке в редакторе.
    /// </summary>
    private void HighlightLineInEditor(int line)
    {
      if (line <= 0 || _translationControl == null)
        return;

      // Берём диспетчер приложения
      var dispatcher = Application.Current?.Dispatcher;

      // Если по каким-то причинам диспетчера нет – выходим
      if (dispatcher == null)
        return;

      void DoHighlight()
      {
        var type = _translationControl.GetType();

        try
        {
          var setActive = type.GetMethod("SetActiveLine", new[] { typeof(int) });
          setActive?.Invoke(_translationControl, new object[] { line });
        }
        catch { /* игнорируем, чтобы не ронять исполнение */ }

        try
        {
          var scroll = type.GetMethod("ScrollToLine", new[] { typeof(int) });
          scroll?.Invoke(_translationControl, new object[] { line });
        }
        catch { /* игнорируем */ }
      }

      if (dispatcher.CheckAccess())
      {
        // Уже в UI-потоке
        DoHighlight();
      }
      else
      {
        // Уводим на UI-поток, НЕ блокируя текущий
        dispatcher.BeginInvoke((Action)DoHighlight);
      }
    }

    /// <summary>
    /// Выполняет все команды по очереди, с поддержкой точек останова.
    /// Остановки только на заголовках команд (FormattedStartLineNumber).
    /// </summary>
    public async Task ExecuteAllAsync(CancellationToken cancellationToken = default)
    {
      for (int i = 0; i < CommandsToExecute.Count; i++)
      {
        cancellationToken.ThrowIfCancellationRequested();

        var command = CommandsToExecute[i];
        var headerLine = command.FormattedStartLineNumber;

        LoggerUtility.LogInformation($"[CEM] cmd {command.CommandNumber} headerLine = {headerLine}");

        if (_headerBreakpoints.Contains(headerLine))
        {
          HighlightLineInEditor(headerLine);
          await WaitOnBreakpointAsync(cancellationToken);
        }

        var context = new CommandExecutionContext(this, command, _console, _translationControl, _opkFilePath)
        {
          JumpToCommandNumber = (number) =>
          {
            int newIndex = CommandsToExecute.FindIndex(cmd => cmd.CommandNumber == number);
            if (newIndex >= 0)
            {
              // -1, потому что в конце цикла i++.
              i = newIndex - 1;
            }
          }
        };

        if (_executors.TryGetValue(command.Mnemonic, out var executor))
        {
          await executor.ExecuteAsync(context, _protocolModel);
        }
        else
        {
          await _console.ShowMessageAsync(new ShowMessageModel(
            "Неизвестная команда",
            message: command.Mnemonic,
            type: ShowMessageModel.MessageType.Error));
        }
      }
    }

    /// <summary>
    /// Ожидание продолжения после остановки на breakpoint.
    /// </summary>
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

      return tcs.Task;
    }

    /// <summary>
    /// Продолжить выполнение после остановки на breakpoint.
    /// Вызывается из UI (F8, кнопка "Следующая точка" и т.п.).
    /// </summary>
    public void ContinueFromBreakpoint()
    {
      _breakpointTcs?.TrySetResult(true);
    }

    /// <summary>
    /// Выполняет одну команду по предоставленной модели (без учёта breakpoints).
    /// </summary>
    public async Task ExecuteOneAsync(BaseCommandModel command)
    {
      if (_executors.TryGetValue(command.Mnemonic, out var executor))
      {
        var context = new CommandExecutionContext(this, command, _console, _translationControl, _opkFilePath);
        await executor.ExecuteAsync(context, _protocolModel);
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
    /// Пропускает команды до указанного номера и продолжает выполнение с неё
    /// (без учёта breakpoints).
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