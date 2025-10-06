using Utilities.Interface;

namespace NewCore.Base.Function.DBC
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
    Task<bool> ConnectCapacitor(int number, IUserMessageService? userMessageService = null);

    /// <summary>
    /// Отключение конденсаторов.
    /// </summary>
    /// <param name="number">Номер конденсатора.</param>
    /// <returns>Возвращает результат отключения.</returns>
    Task<bool> DisconnectCapacitor(int number, IUserMessageService? userMessageService = null);
  }
}
