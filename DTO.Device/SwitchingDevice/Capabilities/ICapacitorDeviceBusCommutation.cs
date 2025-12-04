using DTO.Service;

namespace DTO.Device.SwitchingDevice.Capabilities
{
  /// <summary>
  /// Управление коммутацией конденсаторов.
  /// </summary>
  public interface ICapacitorDeviceBusCommutation
  {
    /// <summary>
    /// Подключение конденсаторов.
    /// </summary>
    /// <param name="number">Номер конденсатора.</param>
    /// <returns>Возвращает результат подключения.</returns>
    Task<bool> ConnectCapacitor(int number, IUserInteractionService? userMessageService = null);

    /// <summary>
    /// Отключение конденсаторов.
    /// </summary>
    /// <param name="number">Номер конденсатора.</param>
    /// <returns>Возвращает результат отключения.</returns>
    Task<bool> DisconnectCapacitor(int number, IUserInteractionService? userMessageService = null);
  }
}
