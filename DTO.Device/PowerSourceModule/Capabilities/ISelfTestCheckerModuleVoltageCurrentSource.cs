using DTO.Device.FastMeter;
using DTO.Device.SwitchingDevice;
using Utilities.Interface;

namespace DTO.Device.PowerSourceModule.Capabilities
{
  public interface ISelfTestCheckerModuleVoltageCurrentSource
  {
    /// <summary>
    /// Запуск самоконтроля устройства коммутации шин.
    /// </summary>
    /// <param name="messageService"></param>
    /// <returns></returns>
    Task StartSelfCheck(CancellationToken cancellationToken, IUserMessageService messageService, System.Enum selectedType, ISwitchingDevice device = null, IPowerSourceModule powerDevice = null, IFastMeter meter = null);

    /// <summary>
    /// Возвращает тип перечисления, используемый как тип проверки.
    /// </summary>
    Type GetTestTypeEnum();
  }
}
