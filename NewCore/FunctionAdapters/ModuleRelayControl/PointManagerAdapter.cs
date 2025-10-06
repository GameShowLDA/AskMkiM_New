using System;
using System.Threading.Tasks;
using AppConfiguration.Error.Device.ModuleRelayControl;
using NewCore.Base.Function.ModuleRelayControl;
using NewCore.Base.Interface.Main;
using NewCore.Function.Helpers;
using NewCore.Function.ModuleRelayControl;
using Utilities.Interface;
using static NewCore.Enum.DeviceEnum;

namespace NewCore.FunctionAdapters.ModuleRelayControl
{
  /// <summary>
  /// Адаптер для управления точками (реле) модуля МКР с отображением сообщений.
  /// </summary>
  internal class PointManagerAdapter : IPointManager
  {
    private readonly IRelaySwitchModule _moduleRelayControl;
    private readonly PointManager _pointManager;


    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="PointManagerAdapter"/>.
    /// </summary>
    /// <param name="moduleRelayControl">Экземпляр модуля реле.</param>
    public PointManagerAdapter(IRelaySwitchModule moduleRelayControl)
    {
      _moduleRelayControl = moduleRelayControl ?? throw new ArgumentNullException(nameof(moduleRelayControl));
      _pointManager = new PointManager(moduleRelayControl);
    }

    /// <inheritdoc />
    public async Task<bool> ConnectRelayAsync(BusPoint bus, int number, IUserMessageService? userMessageService = null)
    {
      var result = await _pointManager.ConnectRelayAsync(bus, number);
      var description = $"{number} к шине [{bus}]";

      if (!result)
        result = await _pointManager.ConnectRelayAsync(bus, number);

      await DeviceMessageBuilder.ShowConnectionMessageAsync(
          _moduleRelayControl,
          $"Подключение точки {description}",
          result,
          1, userMessageService);

      if (!result)
      {
        throw RelayExceptionFactory.ConnectPointFailed(description);
      }

      return result;
    }

    /// <inheritdoc />
    public async Task<bool> DisconnectRelayAsync(BusPoint bus, int number, IUserMessageService? userMessageService = null)
    {


      var result = await _pointManager.DisconnectRelayAsync(bus, number);
      var description = $"{number} от шины [{bus}]";

      if (!result)
        result = await _pointManager.DisconnectRelayAsync(bus, number);



      await DeviceMessageBuilder.ShowConnectionMessageAsync(
          _moduleRelayControl,
          $"Отключение точки {description}",
          result,
          1, userMessageService);

      if (!result)
      { 
        throw RelayExceptionFactory.DisconnectPointFailed(description);
      }
      return result;
    }

    /// <inheritdoc />
    public async Task<bool> ConnectRelayGroupAsync(BusPoint bus, int firstPoint, int lastPoint, IUserMessageService? userMessageService = null)
    {
      var result = await _pointManager.ConnectRelayGroupAsync(bus, firstPoint, lastPoint);
      var description = $"{firstPoint}-{lastPoint} к шине [{bus}]";

      if (!result)
        result = await _pointManager.ConnectRelayGroupAsync(bus, firstPoint, lastPoint);

      await DeviceMessageBuilder.ShowConnectionMessageAsync(
          _moduleRelayControl,
          $"Подключение диапазона точек {description}",
          result,
          1, userMessageService);

      if (!result)
        throw RelayExceptionFactory.ConnectRangeFailed(description);

      return result;
    }

    /// <inheritdoc />
    public async Task<bool> DisconnectRelayGroupAsync(BusPoint bus, int firstPoint, int lastPoint, IUserMessageService? userMessageService = null)
    {
      var result = await _pointManager.DisconnectRelayGroupAsync(bus, firstPoint, lastPoint);
      var description = $"{firstPoint}-{lastPoint} от шины [{bus}]";

      if (!result)
        result = await _pointManager.DisconnectRelayGroupAsync(bus, firstPoint, lastPoint);

      await DeviceMessageBuilder.ShowConnectionMessageAsync(
        _moduleRelayControl,
        $"Отключение диапазона точек {description}",
        result,
        1, userMessageService);

      if (!result)
        throw RelayExceptionFactory.DisconnectRangeFailed(description);

      return result;
    }

    /// <inheritdoc />
    public async Task<bool> DisconnectingAllPoint(IUserMessageService? userMessageService = null)
    {
      var result = await _pointManager.DisconnectingAllPoint(userMessageService);
      var description = $"всех точек от всех шин";

      await DeviceMessageBuilder.ShowConnectionMessageAsync(
          _moduleRelayControl,
          $"Отключение {description}",
          result,
          1, userMessageService);

      if (!result)
        throw RelayExceptionFactory.DisconnectRangeFailed(description);

      return result;
    }


    /// <inheritdoc />
    public async Task<string> CheckPoint(int numberPoint, IUserMessageService? userMessageService = null)
    {
      // TODO : Обработка команды
      return await _pointManager.CheckPoint(numberPoint);
    }

    /// <inheritdoc />
    public async Task<bool> ConnectingPointToNewBus(BusPoint bus, int nubmerPoint, IUserMessageService? userMessageService = null)
    {
      var result = await _pointManager.ConnectingPointToNewBus(bus, nubmerPoint);
      var description = $"{nubmerPoint} к шине [{bus}]";

      if (!result)
        result = await _pointManager.ConnectingPointToNewBus(bus, nubmerPoint);

      await DeviceMessageBuilder.ShowConnectionMessageAsync(
        _moduleRelayControl,
        $"Переподключение точки {description}",
        result,
        1, userMessageService);

      if (!result)
      {
        throw RelayExceptionFactory.ConnectingPointToNewBusFailed(description);
      }

      return result;
    }
  }
}
