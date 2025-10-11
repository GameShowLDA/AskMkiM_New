using DTO.Service;

namespace DTO.Device.FastMeter.Capabilities
{
  /// <summary>
  /// Интерфейс для измерения переменного напряжения.
  /// </summary>
  public interface IAcVoltageMeasurement
  {
    /// <summary>
    /// Устанавливает режим измерения переменного напряжения.
    /// </summary>
    Task<bool> SetACVoltageModeAsync(IUserMessageService? userMessageService = null);

    /// <summary>
    /// Измеряет переменное напряжение.
    /// </summary>
    /// <param name="param">Ожидаемое значение.</param>
    Task<double> MeasureACVoltageAsync(double param = 0, IUserMessageService? userMessageService = null);
  }
}
