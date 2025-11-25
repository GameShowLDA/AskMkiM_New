using DTO.Device.Base;
using DTO.Device.PowerSourceModule;
using DTO.Service;
using Errors.Device;
using Errors.Device.Adapters;
using NewCore.Function.Helpers;
using NewCore.Function.ModuleVoltageCurrentSource;

namespace NewCore.FunctionAdapters.ModuleVoltageCurrentSource
{
  /// <summary>
  /// Адаптер для управления состоянием МИНТ с отображением сообщений.
  /// </summary>
  internal class StateManagerAdapter : IConnectable
  {
    private readonly IPowerSourceModule _device;
    private readonly StateManager _stateManager;

    public StateManagerAdapter(IPowerSourceModule device)
    {
      _device = device ?? throw new ArgumentNullException(nameof(device));
      _stateManager = new StateManager(device);
    }

    public event Action DeviceDisponce;
    public event Action IsReset;

    /// <inheritdoc />
    public async Task<(bool Connect, string Answer)> ConnectAsync(IUserMessageService messageService = null)
    {
      var (success, message) = await _stateManager.ConnectAsync();

      if (!success || await AppConfiguration.DeviceDisplay.DeviceDisplayConfig.GetExecutionParametersVisibilityAsync())
      {
        await DeviceMessageBuilder.ShowConnectionMessageAsync(_device, "Подключение", message, success, 1, messageService);
      }

      if (!success)
        throw ConnectionExceptionAdapter.ConnectFailed(_device.Name, _device.NumberChassis, _device.Number, message);

      return (success, message);
    }

    /// <inheritdoc />
    public async Task<bool> DisconnectAsync(IUserMessageService messageService = null)
    {
      bool success = await _stateManager.DisconnectAsync();

      if (!success || await AppConfiguration.DeviceDisplay.DeviceDisplayConfig.GetExecutionParametersVisibilityAsync())
      {
        await DeviceMessageBuilder.ShowConnectionMessageAsync(_device, "Отключение", success, 1);
      }

      if (!success)
        throw ConnectionExceptionAdapter.DisconnectFailed(_device.Name, _device.NumberChassis, _device.Number);

      return success;
    }

    /// <inheritdoc />
    public async Task<(bool Connect, string Answer)> InitializeAsync(IUserMessageService messageService = null)
    {
      var (success, message) = await _stateManager.InitializeAsync();

      if (!success || await AppConfiguration.DeviceDisplay.DeviceDisplayConfig.GetExecutionParametersVisibilityAsync())
      {
        await DeviceMessageBuilder.ShowConnectionMessageAsync(_device, "Инициализация", message, success, 1, messageService);
      }

      if (!success)
        throw ConnectionExceptionAdapter.InitializeFailed(_device.Name, _device.NumberChassis, _device.Number, message);

      return (success, message);
    }

    /// <inheritdoc />
    public async Task<bool> ResetAsync(IUserMessageService messageService = null)
    {
      bool success = await _stateManager.ResetAsync();

      if (!success || await AppConfiguration.DeviceDisplay.DeviceDisplayConfig.GetExecutionParametersVisibilityAsync())
      {
        await DeviceMessageBuilder.ShowConnectionMessageAsync(_device, "Сброс", success, 1);
      }

      if (!success)
        throw ConnectionExceptionAdapter.ResetFailed(_device.Name, _device.NumberChassis, _device.Number);

      IsReset?.Invoke();
      return success;
    }
  }
}
