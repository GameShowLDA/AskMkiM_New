using DTO.Device.SwitchingDevice;
using Utilities.Interface;

namespace DTO.Device.RelaySwitchModule.Capabilities
{
  /// <summary>
  /// Интерфейс для выполнения проверки модуля коммутации реле.
  /// </summary>
  public interface ISelfTestCheckerModuleRelayControl
  {
    /// <summary>
    /// Запуск самоконтроля модуля коммутации реле.
    /// </summary>
    /// <param name="messageService">Сервис отображения сообщений.</param>
    /// <param name="relaySwitchModule">Модуль коммутации реле.</param>
    /// <param name="device">Устройство коммутации шин.</param>
    /// <param name="meter">Измеритель.</param>
    Task StartSelfCheck(CancellationToken cancellationToken, System.Enum typeConnector, IUserMessageService? userMessageService = null, ISwitchingDevice device = null);


    /// <summary>
    /// Возвращает тип перечисления, используемый как тип проверки.
    /// </summary>
    Type GetTestTypeEnum();

    /// <summary>
    /// Проверяет пару шин A и B по номеру и выдаёт ответ.
    /// </summary>
    /// <param name="numbet"></param>
    /// <returns></returns>
    Task<(bool, string)> TryGetCheckBusConntcrion(int number, IUserMessageService? userMessageService = null);
  }
}
