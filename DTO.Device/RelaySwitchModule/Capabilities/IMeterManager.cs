using DTO.Service;

namespace DTO.Device.RelaySwitchModule.Capabilities

{
  /// <summary>
  /// Интерфейс для управления измерителем в модуле МКР.
  /// </summary>
  public interface IMeterManager
  {
    /// <summary>
    /// Включает измеритель модуля МКР.
    /// </summary>
    /// <returns>Возвращает true, если команда отправлена успешно.</returns>
    Task<bool> ConnectMeterAsync(IUserInteractionService? userMessageService = null);

    /// <summary>
    /// Отключает измеритель модуля МКР.
    /// </summary>
    /// <returns>Возвращает true, если команда отправлена успешно.</returns>
    Task<bool> DisconnectMeterAsync(IUserInteractionService? userMessageService = null);

    /// <summary>
    /// Получает ответ от измерителя о замыкании шин или точек.
    /// </summary>
    /// <returns>True, если есть замыкание, false, если нет.</returns>
    Task<bool> GetMeterResponseAsync(IUserInteractionService? userMessageService = null);
  }
}
