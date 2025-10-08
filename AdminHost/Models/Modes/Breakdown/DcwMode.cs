using AdminHost.Models.Interface;
using NewCore.Base.Interface.Main;

namespace AdminHost.Models.Modes.Breakdown
{
  public class DcwMode : IDeviceMode
  {
    public string Name => "DCW";
    public string Description => "Испытание изоляции постоянным напряжением для проверки прочности.";
    public Type DeviceInterface => typeof(IBreakdownTester);
  }
}
