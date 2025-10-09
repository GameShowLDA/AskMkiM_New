using Utilities.Interface;

namespace DTO.Device.FastMeter.Capabilities
{
  /// <summary>
  /// Интерфейс для проверки проводимости (прозвонки).
  /// </summary>
  public interface IContinuityMeasurement
  {
    /// <summary>
    /// Устанавливает режим прозвонки.
    /// </summary>
    Task<bool> SetContinuityModeAsync(IUserMessageService? userMessageService = null);

    /// <summary>
    /// Проверяет наличие проводимости.
    /// </summary>
    Task<bool> CheckContinuityAsync(bool expectedOutcome, IUserMessageService? userMessageService = null);

    /// <summary>
    /// Проверяет наличие проводимости.
    /// </summary>
    Task<double> CheckContinuityAsync(double expectedOutcome, IUserMessageService? userMessageService = null);
  }
}
