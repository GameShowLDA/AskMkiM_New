using System.ComponentModel.DataAnnotations.Schema;
using DTO.Device.Base;
using DTO.Device.Chassis;
using DTO.Device.Chassis.Capabilities;
using static DTO.Enum.DeviceEnums;

namespace DataBaseConfiguration.Models.Device
{
  /// <summary>
  /// Класс, представляющий сущность менеджера шасси.
  /// </summary>
  public class ChassisManagerEntity : IChassisManager
  {
    /// <inheritdoc />
    public int Id { get; set; }

    /// <inheritdoc />
    public string Name { get; set; }

    /// <inheritdoc />
    public string Description { get; set; }

    /// <inheritdoc />
    public int Number { get; set; }

    /// <inheritdoc />
    public string ConnectionDetails { get; set; }

    /// <inheritdoc />
    public DeviceType DeviceType => DeviceType.ChassisManager;

    /// <inheritdoc />
    public string DeviceClass { get; set; }

    /// <inheritdoc />
    [NotMapped]
    public IPowerManagerChassis PowerManager { get; set; }

    /// <inheritdoc />
    [NotMapped]
    public IConnectable ConnectableManager { get; set; }

    /// <inheritdoc />
    [NotMapped]
    public IDeviceProtocol DeviceProtocol { get; set; }
  }
}
