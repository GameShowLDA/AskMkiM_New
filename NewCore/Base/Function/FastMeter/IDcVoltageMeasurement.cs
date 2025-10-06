using Utilities.Interface;

namespace NewCore.Base.Function.FastMeter
{
  /// <summary>
  /// Интерфейс для измерения постоянного напряжения.
  /// </summary>
  public interface IDcVoltageMeasurement
  {
    /// <summary>
    /// Устанавливает режим измерения постоянного напряжения.
    /// </summary>
    Task<bool> SetDCVoltageModeAsync(IUserMessageService? userMessageService = null);

    /// <summary>
    /// Измеряет постоянное напряжение.
    /// </summary>
    /// <param name="param">Ожидаемое значение.</param>
    Task<double> MeasureDCVoltageAsync(double param = 0, IUserMessageService? userMessageService = null);
  }
}
