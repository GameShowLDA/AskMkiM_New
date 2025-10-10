using AppConfiguration.Error.Device.ModuleRelayControl;
using DTO.Device.RelaySwitchModule;
using DTO.Device.RelaySwitchModule.Capabilities;
using NewCore.Function.Helpers;
using NewCore.Function.ModuleRelayControl;
using DTO.Service;

namespace NewCore.FunctionAdapters.ModuleRelayControl
{
  /// <summary>
  /// Адаптер для управления измерителем модуля МКР с отображением сообщений.
  /// </summary>
  internal class MeterManagerAdapter : IMeterManager
  {
    private readonly IRelaySwitchModule _moduleRelayControl;
    private readonly MeterManager _meterManager;
    private bool IsConnectMeter = false;
    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="MeterManagerAdapter"/>.
    /// </summary>
    /// <param name="moduleRelayControl">Экземпляр модуля реле.</param>
    public MeterManagerAdapter(IRelaySwitchModule moduleRelayControl)
    {
      _moduleRelayControl = moduleRelayControl ?? throw new ArgumentNullException(nameof(moduleRelayControl));
      _meterManager = new MeterManager(moduleRelayControl);
      IsConnectMeter = false;

      moduleRelayControl.ConnectableManager.IsReset += () => IsConnectMeter = false;
    }

    /// <inheritdoc />
    public async Task<bool> ConnectMeterAsync(IUserMessageService? userMessageService = null)
    {
      if (IsConnectMeter)
        return true;

      const string description = "модуля МКР";

      var result = await _meterManager.ConnectMeterAsync();

      await DeviceMessageBuilder.ShowConnectionMessageAsync(
          _moduleRelayControl,
          $"Подключение измерителя {description}",
          result,
          1, userMessageService);

      if (!result)
      {
        throw MeterExceptionFactory.ConnectFailed(description);
      }
      else
      {
        IsConnectMeter = true;
      }

      return result;
    }

    /// <inheritdoc />
    public async Task<bool> DisconnectMeterAsync(IUserMessageService? userMessageService = null)
    {
      if (IsConnectMeter)
        return false;

      const string description = "модуля МКР";

      var result = await _meterManager.DisconnectMeterAsync();

      await DeviceMessageBuilder.ShowConnectionMessageAsync(
          _moduleRelayControl,
          $"Отключение измерителя {description}",
          result,
          1, userMessageService);

      if (!result)
      {
        throw MeterExceptionFactory.DisconnectFailed(description);
      }
      else
      {
        IsConnectMeter = false;
      }

      return result;
    }

    /// <inheritdoc />
    public async Task<bool> GetMeterResponseAsync(IUserMessageService? userMessageService = null)
    {
      return await _meterManager.GetMeterResponseAsync();
    }
  }
}
