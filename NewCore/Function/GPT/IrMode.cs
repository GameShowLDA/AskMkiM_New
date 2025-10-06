using System.Globalization;
using System.Text.RegularExpressions;
using NewCore.Base.Function.Breakdown;
using NewCore.Base.Function.Breakdown.Capabilities;
using NewCore.Device;
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
  /// Класс для управления режимом IR (Insulation Resistance).
  /// </summary>
  public class IrMode : IIrModeBreakdown
  {
    private GPT79904 _gptModel { get; set; }
    private static double timeDelay = 2;
    private static int delayBeforeCall = 100;
    int delay = 100;
    private IrConfiguration _config;

    /// <inheritdoc />
    public IModeConfigurable Mode { get; set; }

    /// <inheritdoc />

    public IVoltageConfigurable Voltage { get; set; }

    /// <inheritdoc />

    public ITimeConfigurable Time { get; set; }

    /// <inheritdoc />
    public IOffsetConfigurable Offset { get; set; }

    /// <inheritdoc />
    public IMeasurable Measure { get; set; }

    /// <inheritdoc />
    public IConfigurationProvider<IrConfiguration> Config { get; set; }

    /// <inheritdoc />
    public IResistanceLimitsConfigurable ResistanceLimits { get; set; }

    public IrMode(GPT79904 gpt79904)
    {
      _gptModel = gpt79904;
      _config = new IrConfiguration();
      Voltage = new VoltageManagment(_gptModel, TypeMode.IR, delay, getConfigVoltage: () => _config.Voltage, setConfigVoltage: v => _config.Voltage = v);
      Time = new TimeManagment(_gptModel, TypeMode.IR, delay, getTestTime: () => _config.TestTime, setTestTime: v => _config.TestTime = v, getRampTime: () => _config.RampTime, setRampTime: v => _config.RampTime = v);
      Offset = new OffsetManagment(_gptModel, TypeMode.IR, delay, getOffset: () => _config.Offset, setOffset: v => _config.Offset = v);
      Measure = new IrMeasureManagment(_gptModel, delayBeforeCall, getTestTime: () => Task.FromResult(_config.TestTime), getRampTime: () => Task.FromResult(_config.RampTime), getIsIdleMode: GetIsIdleModeEnabled);
      ResistanceLimits = new ResistanceLimitsManagment(_gptModel, delay, GetIsIdleModeEnabled, () => _config.HighResistanceLimit, v => _config.HighResistanceLimit = v, () => _config.LowResistanceLimit, v => _config.LowResistanceLimit = v);
      Config = new IrConfigManager(Voltage, Time, Offset, ResistanceLimits.GetHighResistanceLimitAsync, ResistanceLimits.GetLowResistanceLimitAsync);
      Mode = new ModeManagment(_gptModel, TypeMode.IR, delay, async () => _config = await Config.ReadConfigurationAsync());
    }
  }
}
