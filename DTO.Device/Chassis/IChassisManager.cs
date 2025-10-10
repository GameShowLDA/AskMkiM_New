using DTO.Device.Base;
using DTO.Device.Chassis.Capabilities;

namespace DTO.Device.Chassis
{
  /// <summary>
  /// Интерфейс для менеджера шасси.
  /// </summary>
  public interface IChassisManager : IDevice, IHeadUnit
  {
    /// <summary>
    /// Управление питанием шасси.
    /// </summary>
    public IPowerManagerChassis PowerManager { get; set; }
  }
}
