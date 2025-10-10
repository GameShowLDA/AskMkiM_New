using System.ComponentModel.DataAnnotations.Schema;
using DTO.Device.Base;
using DTO.Device.Breakdown;
using DTO.Device.Breakdown.Capabilities;
using DTO.Device.Breakdown.Mode;
using static DTO.Enum.DeviceEnums;

namespace DataBaseConfiguration.Models.Device
{
  /// <summary>
  /// Класс, представляющий сущность пробойной установки.
  /// </summary>
  public class BreakdownTesterEntity : IBreakdownTester
  {
    /// <inheritdoc />
    public int Id { get; set; }

    /// <inheritdoc />
    public int NumberChassis { get; set; }

    /// <inheritdoc />
    public string Name { get; set; }

    /// <inheritdoc />
    public string Description { get; set; }

    /// <inheritdoc />
    public int Number { get; set; }

    /// <inheritdoc />
    public string ConnectionDetails { get; set; }

    /// <inheritdoc />
    public string DeviceClass { get; set; }

    /// <inheritdoc />
    public int MaxVoltage { get; set; }


    /// <inheritdoc />
    public DeviceType DeviceType => DeviceType.BreakdownTester;

    /// <inheritdoc />
    [NotMapped]
    public IAcwModeBreakdown AcwManger { get; set; }

    /// <inheritdoc />
    [NotMapped]
    public IDcwModeBreakdown DcwManger { get; set; }

    /// <inheritdoc />
    [NotMapped]
    public IIrModeBreakdown IrManger { get; set; }

    /// <inheritdoc />
    [NotMapped]
    public ISystemSettingsBreakdown SystemManger { get; set; }

    /// <inheritdoc />
    [NotMapped]
    public IConnectable ConnectableManager { get; set; }

    /// <inheritdoc />
    [NotMapped]
    public IDeviceProtocol DeviceProtocol { get; set; }

    /// <inheritdoc />
    [NotMapped]
    public ISelfTestCheckerBreakdownTester SelfTestManager { get; set; }

    /// <inheritdoc />
    [NotMapped]
    public BreakdownTypeMode Mode { get; set; }
  }
}
