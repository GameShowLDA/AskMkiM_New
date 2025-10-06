using Utilities.Interface;

namespace NewCore.Base.Function.FastMeter
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
