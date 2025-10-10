using AdminHost.Models.Interface;
using DTO.Device.Breakdown;

namespace AdminHost.Models.Modes.Breakdown
{
  /// <summary>
  /// Режим ACW (испытание изоляции переменным напряжением).
  /// </summary>
  public class AcwMode : IDeviceMode
  {
    public string Name => "ACW";
    public string Description => "Испытание изоляции переменным напряжением для проверки прочности.";
    public Type DeviceInterface => typeof(IBreakdownTester);
  }
}
