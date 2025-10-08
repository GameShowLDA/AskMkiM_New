using AdminHost.Models.Interface;
using NewCore.Base.Interface.Main;

namespace AdminHost.Models.Modes.Breakdown
{
  public class SettingsMode : IDeviceMode
  {
    public string Name => "System";

    public string Description => "Системные настройки ППУ";

    public Type DeviceInterface => typeof(IBreakdownTester);
  }
}
