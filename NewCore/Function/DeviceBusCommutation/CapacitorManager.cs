using NewCore.Base.Function.DBC;
using NewCore.Communication;
using NewCore.Function.Helpers;
using Utilities.Interface;
using Utilities.Models;
using static AppConfiguration.Execution.ExecutionConfig;
using static Utilities.LoggerUtility;

namespace NewCore.Function.DeviceBusCommutation
{
  /// <summary>
  /// Менеджер управления подключением конденсаторов.
  /// Обеспечивает подключение и отключение конденсаторов в системе.
  /// </summary>
  public class CapacitorManager : ICapacitorDeviceBusCommutation
  {
    /// <summary>
    /// Устройство коммутации шин.
    /// </summary>
    private readonly Device.DeviceBusCommutation _deviceBusCommutation;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="BusManager"/>.
    /// </summary>
    /// <param name="deviceBusCommutation">Экземпляр устройства коммутации шин.</param>
    public CapacitorManager(Device.DeviceBusCommutation deviceBusCommutation) => _deviceBusCommutation = deviceBusCommutation;

    /// <summary>
    /// Замыкание конденсатора.
    /// </summary>
    /// <param name="number">Номер конденсатора.</param>
    /// <returns>Задача (Task), представляющая асинхронную операцию.</returns>
    public async Task<bool> ConnectCapacitor(int number, IUserMessageService? userMessageService = null)
    {
      if (await GetIsIdleModeEnabled())
      {
        return true;
      }

      DeviceCommand command = new DeviceCommand(6, 2, number, 1);
      await _deviceBusCommutation.DeviceProtocol.QueryAsync(command.ToString());
      return true;
    }

    /// <summary>
    /// Размыкание конденсатора.
    /// </summary>
    /// <param name="number">Номер конденсатора.</param>
    /// <returns>Задача (Task), представляющая асинхронную операцию.</returns>
    public async Task<bool> DisconnectCapacitor(int number, IUserMessageService? userMessageService = null)
    {
      var showMessageModel = DeviceMessageBuilder.GetDefaultSettings(_deviceBusCommutation);

      if (await GetIsIdleModeEnabled())
      {
        return true;
      }

      DeviceCommand command = new DeviceCommand(6, 2, number, 2);
      await _deviceBusCommutation.DeviceProtocol.QueryAsync(command.ToString());
      return true;
    }
  }
}
