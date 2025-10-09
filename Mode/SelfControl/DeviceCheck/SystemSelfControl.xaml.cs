using System.Windows.Controls;
using static DTO.Enum.DeviceEnums;

namespace Mode.SelfControl.DeviceCheck
{
  /// <summary>
  /// Логика взаимодействия для SystemSelfControl.xaml
  /// </summary>
  public partial class SystemSelfControl : UserControl
  {
    public SystemSelfControl()
    {
      InitializeComponent();
      InitializeSettings();
    }

    /// <summary>
    /// Инициализирует все необходимые настройки для компонента.
    /// Очищает предыдущий контент и добавляет новые элементы управления.
    /// </summary>
    public void InitializeSettings()
    {
      ProtocolUI.SetSettings(
        this,
        StartDelegate: ExecuteMeasurementProcess,
        true,
        checkPower: false);
    }

    /// <summary>
    /// Выполнение контроля.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    private async Task ExecuteMeasurementProcess(CancellationToken cancellationToken)
    {
      var managerShassi = new DataBaseConfiguration.Services.Device.ChassisManagerServices().GetAllEntities().FirstOrDefault();
      if (managerShassi == null)
      {
        return;
      }

      var meter = new DataBaseConfiguration.Services.Device.FastMeterServices().GetDevicesByNumberChassis(managerShassi.Number).FirstOrDefault();
      if (meter == null)
      {
        return;
      }

      var dbc = new DataBaseConfiguration.Services.Device.SwitchingDeviceServices().GetDevicesByNumberChassis(managerShassi.Number).FirstOrDefault();
      var mkr = new DataBaseConfiguration.Services.Device.RelaySwitchModuleServices().GetDevicesByNumberChassis(managerShassi.Number);

      await dbc.SelfTestManager.StartSelfCheck(ProtocolUI.GetCancellationToken(), SwitchingDeviceTypeConnector.FullCheck, ProtocolUI, dbc, meter);

      foreach (var item in mkr)
      {
        await item.SelfTestManager.StartSelfCheck(ProtocolUI.GetCancellationToken(), RelaySwitchTypeConnector.FullCheck, ProtocolUI, dbc);
      }
    }
  }
}
