using ControlCommandAnalyser.Model;
using ControlCommandAnalyser.Model.Chains;
using ControlCommandExecutor.Execution;
using DTO.Service;
using static ControlCommandExecutor.BaseStrategies.NodeFullChecker;

/// <summary>
/// Представляет контекст выполнения метода измерения в исполнительном модуле.
/// Содержит все необходимые данные, параметры и сервисы для проведения измерения
/// и обработки его результатов.
/// </summary>
internal class MethodExecutionContext
{
  /// <summary>
  /// Модель схемы подключения, используемая для выполнения измерения.
  /// Определяет точки, пары и соединения, участвующие в текущем методе.
  /// </summary>
  internal SchemeModel SchemeModel { get; set; }

  /// <summary>
  /// Делегат, выполняющий операцию измерения.
  /// Вызывается методами исполнительного модуля для получения результата измерения.
  /// </summary>
  internal PerformMeasurementAsync PerformMeasurementAsync { get; set; }

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
}
