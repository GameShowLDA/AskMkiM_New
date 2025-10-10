using DTO.Device.Chassis;
using DTO.Device.Chassis.Capabilities;
using DTO.Enum;
using NewCore.Base.Device;
using NewCore.Function.ManagerChassis;

namespace NewCore.Device
{
  /// <summary>
  /// Класс ManagerChassis представляет устройство с подключением по IP-адресу.
  /// </summary>
  public class ManagerChassis : DeviceWithIP, IChassisManager
  {
    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="ManagerChassis"/>.
    /// </summary>
    public ManagerChassis()
    {
      ConnectableManager = new StateManager(this);
      PowerManager = new PowerManager(this);
      DeviceType = DeviceEnums.DeviceType.ChassisManager;

      Name = "Тестер АСКМ";
      Description = "Добавить описание сюда";
      DeviceClass = GetType().FullName;
    }

    /// <inheritdoc />
    public IPowerManagerChassis PowerManager { get; set; }
  }
}