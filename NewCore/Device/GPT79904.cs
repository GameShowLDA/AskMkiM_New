using System.IO.Ports;
using DTO.Enum;
using NewCore.Base.Device;
using NewCore.Base.Function.Breakdown;
using NewCore.Base.Interface.Additionally;
using NewCore.Base.Interface.Main;
using NewCore.Communication;
using NewCore.Enum;
using NewCore.Function.GPT;
using NewCore.Function.GPT.Data;
using NewCore.FunctionAdapters.GPT;
using static Utilities.LoggerUtility;

namespace NewCore.Device
{
  /// <summary>
  /// Класс, представляющий пробойную установку GPT79904, работающую через последовательный порт (COM).
  /// </summary>
  public class GPT79904 : DeviceWithCOM, IBreakdownTester
  {

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="GPT79904"/>.
    /// </summary>
    public GPT79904()
    {
      BaudRate = 115200;
      StopBits = StopBits.One;
      DataBits = 8;
      Parity = Parity.None;
      DeviceClass = GetType().FullName;

      DeviceType = DeviceEnums.DeviceType.BreakdownTester;

      AcwManger = new AcwModeAdapter(this);
      DcwManger = new DcwModeAdapter(this);
      IrManger = new IrModeAdapter(this);
      SystemManger = new SystemSettingsAdapter(this);
      ConnectableManager = new ConnectableManagerAdapter(this);
      SelfTestManager = new NewCore.Function.GPT.SelfCheck.SelfTestManager();
      MaxVoltage = 600;
      LogWarning($"[{GetType().Name}] ctor вызван. Hash={GetHashCode()}", isDeviceLog: true);

      Mode = TypeMode.None;
    }

    /// <inheritdoc />
    public new string Name { get => "GPT79904"; }

    /// <inheritdoc />
    public new string Description { get => "Реализовать описание в NewCore.Device.GPT79904"; }

    /// <inheritdoc />
    public int NumberChassis { get; set; }

    /// <inheritdoc />
    public IAcwModeBreakdown AcwManger { get; set; }

    /// <inheritdoc />
    public IDcwModeBreakdown DcwManger { get; set; }

    /// <inheritdoc />
    public IIrModeBreakdown IrManger { get; set; }

    /// <inheritdoc />
    public ISystemSettingsBreakdown SystemManger { get; set; }

    /// <inheritdoc />
    public int MaxVoltage { get; set; }

    /// <inheritdoc />
    public ISelfTestCheckerBreakdownTester SelfTestManager { get; set; }

    /// <summary>
    /// Активный режим устройства.
    /// </summary>
    public TypeMode Mode
    {
      get => _mode;
      set
      {
        if (_mode == value)
          return;

        LogInformation($"[{GetType().Name}] Переключение режима: {_mode} → {value}",isDeviceLog: true);

        if (value != TypeMode.ACW)
          AcwManger.Config.ResetConfiguration();
        if (value != TypeMode.DCW)
          DcwManger.Config.ResetConfiguration();
        if (value != TypeMode.IR)
          IrManger.Config.ResetConfiguration();

        _mode = value;
      }
    }

    private TypeMode _mode { get; set; }
  }
}
