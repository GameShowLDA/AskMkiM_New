using AdminHost.Models.Interface;
using DTO.Device.Breakdown;

namespace AdminHost.Models.Modes.Breakdown
{
  public class IrMode : IDeviceMode
  {
    public string Name => "IR";

    public string Description => "Испытание изоляции для проверки сопротивления";

    public Type DeviceInterface => typeof(IBreakdownTester);
  }
}
