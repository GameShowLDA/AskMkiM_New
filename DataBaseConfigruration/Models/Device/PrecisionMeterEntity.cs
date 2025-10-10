using System.ComponentModel.DataAnnotations.Schema;
using DTO.Device.Base;
using DTO.Device.PrecisionMeter;
using static DTO.Enum.DeviceEnums;

namespace DataBaseConfiguration.Models.Device
{
  /// <summary>
  /// Класс, представляющий сущность точного измерителя.
  /// </summary>
  public class PrecisionMeterEntity : IPrecisionMeter
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
    public DeviceType DeviceType => DeviceType.PrecisionMeter;

    /// <inheritdoc />
    public string DeviceClass { get; set; }

    /// <inheritdoc />
    [NotMapped]
    public IConnectable ConnectableManager { get; set; }

    /// <inheritdoc />
    [NotMapped]
    public IDeviceProtocol DeviceProtocol { get; set; }
  }
}
