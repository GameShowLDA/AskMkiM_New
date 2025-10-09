using Utilities.Interface;
using static DTO.Enum.DeviceEnums;

namespace DTO.Device.PowerSourceModule.Capabilities
{
  /// <summary>
  /// Интерфейс для управления подключением шин к положительной и отрицательной полярности.
  /// </summary>
  public interface IBusManager
  {
    /// <summary>
    /// Подключает указанную шину к положительной полярности.
    /// </summary>
    /// <param name="bus">Шина, которую нужно подключить.</param>
    /// <returns>Задача, содержащая результат операции (true, если успешно).</returns>
    Task<bool> ConnectBusToPositiveAsync(SwitchingBus bus, IUserMessageService? userMessageService = null);

    /// <summary>
    /// Подключает указанную шину к отрицательной полярности.
    /// </summary>
    /// <param name="bus">Шина, которую нужно подключить.</param>
    /// <returns>Задача, содержащая результат операции (true, если успешно).</returns>
    Task<bool> ConnectBusToNegativeAsync(SwitchingBus bus, IUserMessageService? userMessageService = null);

    /// <summary>
    /// Отключает указанную шину от положительной полярности.
    /// </summary>
    /// <param name="bus">Шина, которую нужно отключить.</param>
    /// <returns>Задача, содержащая результат операции (true, если успешно).</returns>
    Task<bool> DisconnectBusToPositiveAsync(SwitchingBus bus, IUserMessageService? userMessageService = null);

    /// <summary>
    /// Отключает указанную шину от отрицательной полярности.
    /// </summary>
    /// <param name="bus">Шина, которую нужно отключить.</param>
    /// <returns>Задача, содержащая результат операции (true, если успешно).</returns>
    Task<bool> DisconnectBusToNegativeAsync(SwitchingBus bus, IUserMessageService? userMessageService = null);
  }
}
