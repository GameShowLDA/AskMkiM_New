using Utilities.Interface;

namespace DTO.Device.FastMeter.Capabilities
{
  /// <summary>
  /// Определяет интерфейс для измерения сопротивления, включая установку режима измерения и выполнение измерения.
  /// </summary>
  public interface IResistanceMeasurement
  {
    /// <summary>
    /// Асинхронно устанавливает режим измерения сопротивления.
    /// </summary>
    /// <returns>Задача, завершающаяся после установки режима.</returns>
    Task<bool> SetResistanceModeAsync(IUserMessageService? userMessageService = null);

    /// <summary>
    /// Асинхронно выполняет измерение сопротивления.
    /// </summary>
    /// <returns>Задача, возвращающая измеренное значение сопротивления в Омах.</returns>
    /// <param name="param">Ожидаемое значение.</param>
    Task<double> MeasureResistanceAsync(double param = 0, double rangeFrom = -1, double rangeTo = -1, IUserMessageService? userMessageService = null);
  }
}
