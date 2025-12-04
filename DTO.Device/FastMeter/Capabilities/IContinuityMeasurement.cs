using DTO.Service;

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
    Task<bool> SetContinuityModeAsync(IUserInteractionService? userMessageService = null);

    /// <summary>
    /// Проверяет наличие проводимости.
    /// </summary>
    Task<bool> CheckContinuityAsync(bool expectedOutcome, IUserInteractionService? userMessageService = null);

    /// <summary>
    /// Проверяет наличие проводимости.
    /// </summary>
    Task<double> CheckContinuityAsync(double expectedOutcome, IUserInteractionService? userMessageService = null);
  }
}
