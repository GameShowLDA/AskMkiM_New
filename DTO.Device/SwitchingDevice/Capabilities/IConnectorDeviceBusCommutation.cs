using Utilities.Interface;
using static DTO.Enum.DeviceEnums;

namespace DTO.Device.SwitchingDevice.Capabilities
{
  /// <summary>
  /// Интерфейс для управления коммутацией устройств на шинах.
  /// </summary>
  public interface IConnectorDeviceBusCommutation
  {
    /// <summary>
    /// Подключает мультиметр к указанной шине.
    /// </summary>
    /// <param name="bus">Шина, к которой подключается мультиметр.</param>
    /// <returns>Возвращает <c>true</c>, если операция выполнена успешно, иначе <c>false</c>.</returns>
    Task<bool> ConnectMultimeter(SwitchingBusNew bus, IUserMessageService? userMessageService = null);

    /// <summary>
    /// Отключает мультиметр от указанной шины.
    /// </summary>
    /// <param name="bus">Шина, от которой отключается мультиметр.</param>
    /// <returns>Возвращает <c>true</c>, если операция выполнена успешно, иначе <c>false</c>.</returns>
    Task<bool> DisconnectMultimeter(SwitchingBusNew bus, IUserMessageService? userMessageService = null);

    /// <summary>
    /// Подключает прецизионный источник напряжения и тока (ПИНТ) к указанной шине.
    /// </summary>
    /// <param name="bus">Шина, к которой подключается ПИНТ.</param>
    /// <returns>Возвращает <c>true</c>, если операция выполнена успешно, иначе <c>false</c>.</returns>
    Task<bool> ConnectPINT(SwitchingBusNew bus, IUserMessageService? userMessageService = null);

    /// <summary>
    /// Отключает прецизионный источник напряжения и тока (ПИНТ) от указанной шины.
    /// </summary>
    /// <param name="bus">Шина, от которой отключается ПИНТ.</param>
    /// <returns>Возвращает <c>true</c>, если операция выполнена успешно, иначе <c>false</c>.</returns>
    Task<bool> DisconnectPINT(SwitchingBusNew bus, IUserMessageService? userMessageService = null);

    /// <summary>
    /// Подключает пробойную установку к системе.
    /// </summary>
    /// <returns>Возвращает <c>true</c>, если операция выполнена успешно, иначе <c>false</c>.</returns>
    Task<bool> ConnectBreakdownTester(IUserMessageService? userMessageService = null);

    /// <summary>
    /// Отключает пробойную установку от системы.
    /// </summary>
    /// <returns>Возвращает <c>true</c>, если операция выполнена успешно, иначе <c>false</c>.</returns>
    Task<bool> DisconnectBreakdownTester(IUserMessageService? userMessageService = null);

    /// <summary>
    /// Подлючает все шины устрйоства.
    /// </summary>
    Task<bool> ConnectAllBuses(IUserMessageService? userMessageService = null);

    /// <summary>
    /// Отключает все шины устройства.
    /// </summary>
    Task<bool> DisconnectAllBuses(IUserMessageService? userMessageService = null);

    /// <summary>
    /// Возвращает результат проверки замкнутой цепи.
    /// </summary>
    /// <param name="mode">Проверяемая цепь.</param>
    Task<bool> GetSuccesCurrentMode(SwitchingDeviceTypeConnector mode, IUserMessageService? userMessageService = null);

    /// <summary>
    /// Подлючаект пробойную установку и мультиметр.
    /// </summary>
    /// <param name="userMessageService">Сервис для вывода сообщений.</param>
    Task<bool> ConnectBreakdownTesterAndMultimeter(IUserMessageService? userMessageService = null);

    /// <summary>
    /// Отключает пробойную установку и мультиметр.
    /// </summary>
    /// <param name="userMessageService">Сервис для вывода сообщений.</param>
    Task<bool> DisconnectBreakdownTesterAndMultimeter(IUserMessageService? userMessageService = null);
  }
}

