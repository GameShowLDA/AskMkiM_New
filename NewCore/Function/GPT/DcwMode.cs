using System.Globalization;
using System.Text.RegularExpressions;
using NewCore.Base.Function.Breakdown;
using NewCore.Base.Function.Breakdown.Capabilities;
using NewCore.Device;
using NewCore.Function.GPT.Command;
using NewCore.Function.GPT.Data;
using NewCore.Function.GPT.Helper;
using NewCore.Function.GPT.Managment;
using Utilities.Interface;
using static AppConfiguration.Execution.ExecutionConfig;
using static NewCore.Function.GPT.Command.FunctionCommandManager;
using static NewCore.Function.GPT.Command.ManualCommandManager;
using static Utilities.LoggerUtility;

namespace NewCore.Function.GPT
{
  /// <summary>
  /// Класс для управления режимом DCW.
  /// </summary>
  public class DcwMode : IDcwModeBreakdown
  {
    /// <summary>
    /// Экземпляр модели устройства <see cref="GPT79904"/>, 
    /// через который выполняются команды и осуществляется обмен данными с прибором.
    /// </summary>
    private GPT79904 _gptModel { get; set; }

    /// <summary>
    /// Задержка (в миллисекундах), выполняемая перед отправкой управляющих команд на устройство.
    /// Используется для обеспечения корректного ответа прибора на последовательные запросы.
    /// </summary>
    private static int delayBeforeCall = 100;

    /// <summary>
    /// Текущая конфигурация режима DCW (постоянный ток высокого напряжения),
    /// содержащая параметры, считанные с устройства или установленные пользователем.
    /// </summary>
    private DcwConfiguration _config;

    /// <summary>
    /// Базовое значение задержки (в миллисекундах), используемое между командами
    /// при взаимодействии с устройством.
    /// </summary>
    private int delay = 50;

    /// <inheritdoc />
    public IModeConfigurable Mode { get; set; }

    /// <inheritdoc />
    public IVoltageConfigurable Voltage { get; set; }

    /// <inheritdoc />
    public ICurrentLimitsConfigurable CurrentLimits { get; set; }

    /// <inheritdoc />
    public ITimeConfigurable Time { get; set; }

    /// <inheritdoc />
    public IOffsetConfigurable Offset { get; set; }

    /// <inheritdoc />
    public IArcCurrentConfigurable ArcCurrent { get; set; }

    /// <inheritdoc />
    public IMeasurable Measure { get; set; }
    /// <inheritdoc />
    public IConfigurationProvider<DcwConfiguration> Config { get; set; }

    /// <summary>
    /// Создаёт новый экземпляр класса <see cref="DcwMode"/>.
    /// </summary>
    /// <param name="gpt79904">
    /// Экземпляр модели устройства <see cref="GPT79904"/>, 
    /// который будет использоваться для управления режимом DCW.
    /// </param>
    public DcwMode(GPT79904 gpt79904)
    {
      _gptModel = gpt79904;
      _config = new DcwConfiguration();
      Voltage = new VoltageManagment(_gptModel, TypeMode.DCW, delay, getConfigVoltage: () => _config.Voltage, setConfigVoltage: v => _config.Voltage = v);
      CurrentLimits = new CurrentLimitManagment(_gptModel, TypeMode.DCW, delay, getHighLimit: () => _config.HighCurrentLimit, setHighLimit: v => _config.HighCurrentLimit = v, getLowLimit: () => _config.LowCurrentLimit, setLowLimit: v => _config.LowCurrentLimit = v);
      Time = new TimeManagment(_gptModel, TypeMode.DCW, delay, getTestTime: () => _config.TestTime, setTestTime: v => _config.TestTime = v, getRampTime: () => _config.RampTime, setRampTime: v => _config.RampTime = v);
      Offset = new OffsetManagment(_gptModel, TypeMode.DCW, delay, getOffset: () => _config.Offset, setOffset: v => _config.Offset = v);
      ArcCurrent = new ArcCurrentManagment(_gptModel, TypeMode.DCW, delay, getArcCurrent: () => _config.ArcCurrent, setArcCurrent: v => _config.ArcCurrent = v);
      Measure = new MeasureManagment(_gptModel, delayBeforeCall, getTestTime: async () => await Time.GetTestTimeAsync(), getRampTime: async () => await Time.GetRampTimeAsync(), getIsIdleMode: async () => await GetIsIdleModeEnabled());
      Config = new DcwConfigManager(Voltage, CurrentLimits, Time, Offset, ArcCurrent);
      Mode = new ModeManagment(_gptModel, TypeMode.DCW, delay, async () => _config = await Config.ReadConfigurationAsync());
    }

  }
}
