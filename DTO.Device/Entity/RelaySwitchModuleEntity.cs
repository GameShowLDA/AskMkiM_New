using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DTO.Device.Base;
using DTO.Device.RelaySwitchModule;
using DTO.Device.RelaySwitchModule.Capabilities;
using static DTO.Enum.DeviceEnums;

namespace DTO.Device.Entity
{
  /// <summary>
  /// Класс, представляющий сущность модуля коммутации реле.
  /// </summary>
  public class RelaySwitchModuleEntity : IRelaySwitchModule
  {
    [Key]
    /// <inheritdoc />
    public int Id { get; set; }

    /// <inheritdoc />
    public int NumberChassis { get; set; }

    /// <inheritdoc />
    public int NumberRack { get; set; }

    /// <inheritdoc />
    public int PointCount { get; set; }

    /// <inheritdoc />
    public string Name { get; set; }

    /// <inheritdoc />
    public string Description { get; set; }

    /// <inheritdoc />
    public int Number { get; set; }

    /// <inheritdoc />
    public string ConnectionDetails { get; set; }

    /// <inheritdoc />
    public DeviceType DeviceType => DeviceType.RelaySwitchModule;

    /// <inheritdoc />
    public string DeviceClass { get; set; }

    /// <inheritdoc />
    [NotMapped]
    public IBusManager BusManager { get; set; }

    /// <inheritdoc />
    [NotMapped]
    public IMeterManager MeterManager { get; set; }

    /// <inheritdoc />
    [NotMapped]
    public IPointManager PointManager { get; set; }

    /// <inheritdoc />
    [NotMapped]
    public IStateManager StateManager { get; set; }

    /// <inheritdoc />
    [NotMapped]
    public IConnectable ConnectableManager { get; set; }

    /// <inheritdoc />
    [NotMapped]
    public IDeviceProtocol DeviceProtocol { get; set; }

    /// <inheritdoc />
    [NotMapped]
    public ISelfTestCheckerModuleRelayControl SelfTestManager { get; set; }
  }
}
