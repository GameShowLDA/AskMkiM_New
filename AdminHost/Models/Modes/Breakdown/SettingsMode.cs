using AdminHost.Models.Interface;
using DTO.Device.Breakdown;

namespace AdminHost.Models.Modes.Breakdown
{
  public class SettingsMode : IDeviceMode
  {
    public string Name => "System";

    public string Description => "Системные настройки ППУ";

    public Type DeviceInterface => typeof(IBreakdownTester);
  }
}
