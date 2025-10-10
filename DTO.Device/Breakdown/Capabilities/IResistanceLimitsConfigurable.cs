using System.Threading.Tasks;
using DTO.Service;

namespace DTO.Device.Breakdown.Capabilities
{
  /// <summary>
  /// Интерфейс для режимов, поддерживающих установку и получение пределов сопротивления (IR).
  /// </summary>
  public interface IResistanceLimitsConfigurable
  {
    /// <summary>
    /// Устанавливает верхний предел сопротивления (в ГОм).
    /// </summary>
    /// <param name="value">Значение верхнего предела в ГОм.</param>
    /// <param name="userMessageService">Необязательный сервис сообщений пользователю.</param>
    Task<(bool Success, string Message)> SetHighResistanceLimitAsync(double value, IUserMessageService? userMessageService = null);

    /// <summary>
    /// Считывает верхний предел сопротивления (в ГОм).
    /// </summary>
    Task<double> GetHighResistanceLimitAsync();

    /// <summary>
    /// Устанавливает нижний предел сопротивления (в МОм).
    /// </summary>
    /// <param name="value">Значение нижнего предела в МОм.</param>
    /// <param name="userMessageService">Необязательный сервис сообщений пользователю.</param>
    Task<(bool Success, string Message)> SetLowResistanceLimitAsync(double value, IUserMessageService? userMessageService = null);

    /// <summary>
    /// Считывает нижний предел сопротивления (в МОм).
    /// </summary>
    Task<double> GetLowResistanceLimitAsync();
  }
}
