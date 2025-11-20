using ControlCommandAnalyser.Model;
using ControlCommandAnalyser.Model.Chains;
using ControlCommandExecutor.Execution;
using DTO.Service;

namespace ControlCommandExecutor.BaseStrategies.Data
{
  internal abstract class ExecutorContext
  {
    /// <summary>
    /// Модель схемы подключения, используемая для выполнения измерения.
    /// Определяет точки, пары и соединения, участвующие в текущем методе.
    /// </summary>
    internal SchemeModel SchemeModel { get; set; }

    /// <summary>
    /// Менеджер выполнения команд, обеспечивающий координацию выполнения,
    /// обработку ошибок и последовательность выполнения операций.
    /// </summary>
    internal CommandExecutionManager CommandManager { get; set; }

    /// <summary>
    /// Модель команды, содержащая параметры,
    /// структуру и настройки текущей команды измерения.
    /// </summary>
    internal BaseCommandModel CommandModel { get; set; }

    /// <summary>
    /// Сервис пользовательских сообщений, используемый для отображения
    /// информационных сообщений, предупреждений и ошибок в процессе выполнения измерения.
    /// </summary>
    internal IUserMessageService MessageService { get; set; }

    /// <summary>
    /// Номинальное значение сопротивления, используемое при выполнении измерения
    /// или сравнении результата с заданными допусками.
    /// </summary>
    internal double Resistance { get; set; }

    /// <summary>
    /// Нижний предел допустимого значения измеряемого параметра.
    /// Применяется для проверки результата после выполнения измерения.
    /// </summary>
    public double LowerLimit { get; set; }

    /// <summary>
    /// Верхний предел допустимого значения измеряемого параметра.
    /// Представлен в текстовом виде, если значение содержит специальные обозначения
    /// или форматируется в нестандартной форме.
    /// </summary>
    public double HigherLimit { get; set; }

    public string UnitMnemonic { get; set; }
    public string Unit { get; set; }

    protected void CopyFrom(ExecutorContext other)
    {
      SchemeModel = other.SchemeModel;
      CommandManager = other.CommandManager;
      CommandModel = other.CommandModel;
      MessageService = other.MessageService;
      Resistance = other.Resistance;
      LowerLimit = other.LowerLimit;
      HigherLimit = other.HigherLimit;
      Unit = other.Unit;
      UnitMnemonic = other.UnitMnemonic;
    }

    public T CreateChild<T>() where T : ExecutorContext, new()
    {
      var child = new T();
      child.CopyFrom(this);
      return child;
    }
  }
}
