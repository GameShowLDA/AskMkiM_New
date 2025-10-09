using Utilities.Interface;

namespace DTO.Device.FastMeter.Capabilities
{
  /// <summary>
  /// Интерфейс для измерения ёмкости.
  /// </summary>
  public interface ICapacitanceMeasurement
  {
    /// <summary>
    /// Устанавливает режим измерения ёмкости.
    /// </summary>
    Task<bool> SetCapacitanceModeAsync(IUserMessageService? userMessageService = null);

    /// <summary>
    /// Выполняет измерение ёмкости.
    /// </summary>
    /// <param name="param">Ожиданемео знчение.</param>
    Task<double> MeasureCapacitanceAsync(double param = 0, IUserMessageService? userMessageService = null);
  }
}
