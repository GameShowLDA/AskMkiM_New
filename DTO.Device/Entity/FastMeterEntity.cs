using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DTO.Device.Base;
using DTO.Device.FastMeter;
using DTO.Device.FastMeter.Capabilities;
using static DTO.Enum.DeviceEnums;

namespace DTO.Device.Entity
{
  /// <summary>
  /// Класс, представляющий сущность быстрого измерителя.
  /// </summary>
  public class FastMeterEntity : IFastMeter
  {
    [Key]
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
    public DeviceType DeviceType => DeviceType.FastMeter;

    /// <inheritdoc />
    public string DeviceClass { get; set; }

    /// <inheritdoc />
    public int MaxContinuityResistance { get; set; }

    /// <inheritdoc />
    [NotMapped]
    public IAcVoltageMeasurement AcVoltageManager { get; set; }

    /// <inheritdoc />
    [NotMapped]
    public ICapacitanceMeasurement CapacitanceManager { get; set; }

    /// <inheritdoc />
    [NotMapped]
    public IConnection ConnectionManager { get; set; }

    /// <inheritdoc />
    [NotMapped]
    public IContinuityMeasurement ContinuityManager { get; set; }

    /// <inheritdoc />
    [NotMapped]
    public IDcVoltageMeasurement DcVoltageManager { get; set; }

    /// <inheritdoc />
    [NotMapped]
    public IResistanceMeasurement ResistanceManager { get; set; }

    /// <inheritdoc />
    [NotMapped]
    public IConnectable ConnectableManager { get; set; }

    /// <inheritdoc />
    [NotMapped]
    public IDeviceProtocol DeviceProtocol { get; set; }
  }
}
