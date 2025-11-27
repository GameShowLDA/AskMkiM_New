using ControlCommandAnalyser.Model;
using ControlCommandExecutor.Executors;
using DTO.Base.Models;
using DTO.Service;
using Errors.Models;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
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
    private readonly ITextEditorAdapter translationControl;
    private ProtocolModel protocolModel = new ProtocolModel();

    /// <summary>
    /// Событие, которое вызывается при добавлении ошибки.
    /// </summary>
    public event Action<ErrorItem> AddError;

    /// <summary>
    /// Событие, которое вызывается при очистке ошибок.
    /// </summary>
    public event Action ClearError;

    private readonly string? _opkFilePath;

    /// <summary>
    /// Команды программы контроля.
    /// </summary>
    public List<BaseCommandModel> CommandsToExecute { get; set; } = new();

    // ======== ДОБАВЛЕНО ДЛЯ BREAKPOINT'ОВ ========

    /// <summary>
    /// Набор строк, на которых установлены точки останова.
    /// </summary>
    private readonly IReadOnlyCollection<int> _breakpoints;

    /// <summary>
    /// Функция получения номера строки из модели команды (кэш по Reflecton).
    /// </summary>
    private readonly Func<BaseCommandModel, int>? _getLineNumber;

    /// <summary>
    /// TCS для ожидания продолжения выполнения после остановки на breakpoint.
    /// </summary>
    private TaskCompletionSource<bool>? _breakpointTcs;

    // =============================================

    public void ClearErrorsMethod()
    {
      ClearError?.Invoke();
    }

    public void AddErrorMethod(ErrorItem errorItem)
    {
      AddError?.Invoke(errorItem);
    }

    /// <summary>
    /// Создаёт менеджер выполнения команд.
    /// </summary>
    /// <param name="console">Сервис вывода сообщений.</param>
    /// <param name="textEditor">Адаптер текстового редактора.</param>
    /// <param name="controlProgram">Список команд программы контроля.</param>
    /// <param name="opkFilePath">Путь к исходному OPK-файлу.</param>
    /// <param name="breakpoints">
    /// Номера строк, на которых нужно останавливаться (может быть пустым).
    /// Линии должны быть в тех же координатах, что и свойство строки в BaseCommandModel.
    /// </param>
    public CommandExecutionManager(
      IUserMessageService console,
      ITextEditorAdapter textEditor,
      List<BaseCommandModel> controlProgram,
      string? opkFilePath,
      IReadOnlyCollection<int>? breakpoints = null)
    {
      _console = console;
      CommandsToExecute = controlProgram;
      translationControl = textEditor;
      _opkFilePath = opkFilePath;
      _breakpoints = breakpoints ?? Array.Empty<int>();

      _getLineNumber = BuildLineNumberAccessor();
      RegisterExecutors();
    }

    /// <summary>
    /// Построить делегат, вытаскивающий номер строки из BaseCommandModel.
    /// Пытаемся найти одно из свойств: SourceLine, SourseLine, LineNumber, Line.
    /// </summary>
    private Func<BaseCommandModel, int>? BuildLineNumberAccessor()
    {
      var type = typeof(BaseCommandModel);

      var prop =
        type.GetProperty("SourceLine", BindingFlags.Public | BindingFlags.Instance) ??
        type.GetProperty("SourseLine", BindingFlags.Public | BindingFlags.Instance) ?? // на случай опечатки
        type.GetProperty("LineNumber", BindingFlags.Public | BindingFlags.Instance) ??
        type.GetProperty("Line", BindingFlags.Public | BindingFlags.Instance);

      if (prop == null || prop.PropertyType != typeof(int))
        return null;

      return (BaseCommandModel cmd) =>
      {
        var value = prop.GetValue(cmd);
        return value is int i ? i : 0;
      };
    }

    /// <summary>
    /// Получить номер строки для команды. Если свойство не найдено или 0 — считаем, что строки нет.
    /// </summary>
    private int GetLineNumber(BaseCommandModel command)
    {
      if (_getLineNumber == null)
        return 0;

      try
      {
        return _getLineNumber(command);
      }
      catch
      {
        return 0;
      }
    }

    /// <summary>
    /// Подсветить и прокрутить к строке в редакторе, если адаптер это умеет.
    /// (ищем методы SetActiveLine(int) и ScrollToLine(int) через Reflection)
    /// </summary>
    private void HighlightLineInEditor(int line)
    {
      if (line <= 0 || translationControl == null)
        return;

      var type = translationControl.GetType();

      try
      {
        var setActive = type.GetMethod("SetActiveLine", new[] { typeof(int) });
        setActive?.Invoke(translationControl, new object[] { line });
      }
      catch { /* игнорируем ошибки привязки */ }

      try
      {
        var scroll = type.GetMethod("ScrollToLine", new[] { typeof(int) });
        scroll?.Invoke(translationControl, new object[] { line });
      }
      catch { /* игнорируем ошибки привязки */ }
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

        // ===== проверка на breakpoint =====
        if (_breakpoints.Count > 0)
        {
          int line = GetLineNumber(command);
          LoggerUtility.LogInformation($"[CEM] cmd {command.CommandNumber} line = {line}");
          if (line != 0 && _breakpoints.Contains(line))
          {
            // подсветим строку в редакторе, если это возможно
            HighlightLineInEditor(line);

            // ожидаем, пока пользователь нажмёт "Продолжить"
            await WaitOnBreakpointAsync(cancellationToken);
          }
        }

        // Создаём контекст и передаём ссылку на JumpToCommandNumber делегатом/через context
        var context = new CommandExecutionContext(this, command, _console, translationControl, _opkFilePath)
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

    /// <summary>
    /// Ожидание продолжения после остановки на breakpoint.
    /// </summary>
    private Task WaitOnBreakpointAsync(CancellationToken cancellationToken)
    {
      // создаём новый TCS на каждый breakpoint
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
    /// Вызывается из UI (например, кнопка "Продолжить").
    /// </summary>
    public void ContinueFromBreakpoint()
    {
      _breakpointTcs?.TrySetResult(true);
    }

    /// <summary>
    /// Выполняет одну команду по предоставленной модели
    /// (без учёта breakpoints; "шаг" можно сделать через ExecuteAllAsync поверх подсписка).
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
    /// Пропускает команды до указанного номера и продолжает выполнение с неё
    /// (включительно или после неё). Точки останова здесь не учитываются.
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

      // Продолжаем выполнение с найденной команды (с неё или после неё)
      for (int i = index; i < CommandsToExecute.Count; i++)
      {
        await ExecuteOneAsync(CommandsToExecute[i]);
      }
    }
  }
}