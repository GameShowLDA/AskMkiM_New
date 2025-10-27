using DTO.Device.Base;
using DTO.Device.Breakdown.Capabilities;
using DTO.Device.Breakdown.Mode;
using static DTO.Enum.DeviceEnums;

namespace DTO.Device.Breakdown
{
  /// <summary>
  /// Интерфейс для пробойной установки.
  /// </summary>
  public interface IBreakdownTester : IAttachableDevice
  {
    /// <summary>
    /// Активный режим ППУ.
    /// </summary>
    public BreakdownTypeMode Mode { get; set; }

    /// <summary>
    /// Макссимально выдаваемое напряжение.
    /// </summary>
    public int MaxVoltage { get; set; }

    /// <summary>
    /// Минимально выдаваемое напряжение при измерении сопротивления.
    /// </summary>
    public int IRMinVoltage { get; set; }

    /// <summary>
    /// Управление режимом переменного тока (ACW) в пробойной установке.
    /// </summary>
    public IAcwModeBreakdown AcwManger { get; set; }

    /// <summary>
    /// Управление режимом постоянного тока (DCW) в пробойной установке.
    /// </summary>
    public IDcwModeBreakdown DcwManger { get; set; }

    /// <summary>
    /// Управление режимом измерения сопротивления изоляции (IR) в пробойной установке.
    /// </summary>
    public IIrModeBreakdown IrManger { get; set; }

    /// <summary>
    /// Управление системными настройками пробойной установки.
    /// </summary>
    public ISystemSettingsBreakdown SystemManger { get; set; }

    public ISelfTestCheckerBreakdownTester SelfTestManager { get; set; }
  }
}
