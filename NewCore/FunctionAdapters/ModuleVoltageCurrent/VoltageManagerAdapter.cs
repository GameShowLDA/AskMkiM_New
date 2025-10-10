using AppConfiguration.Error.Device.ModuleVoltageCurrent;
using DTO.Device.PowerSourceModule;
using DTO.Device.PowerSourceModule.Capabilities;
using NewCore.Function.Helpers;
using NewCore.Function.ModuleVoltageCurrentSource;
using DTO.Service;
using static DTO.Enum.DeviceEnums;

namespace NewCore.FunctionAdapters.ModuleVoltageCurrentSource
{
  /// <summary>
  /// Адаптер для управления напряжением на МИНТ с отображением сообщений.
  /// </summary>
  internal class VoltageManagerAdapter : IVoltageManager
  {
    private readonly IPowerSourceModule _device;
    private readonly VoltageManager _voltageManager;

    public VoltageManagerAdapter(IPowerSourceModule device)
    {
      _device = device ?? throw new ArgumentNullException(nameof(device));
      _voltageManager = new VoltageManager(device);
    }

    public async Task SetSourceVoltageAsync(VoltageSources voltageSources, IUserMessageService? messageService = null)
    {
      string label = voltageSources == VoltageSources.Supply12V ? "12 В" : "5 В";

      try
      {
        await _voltageManager.SetSourceVoltageAsync(voltageSources);
        await DeviceMessageBuilder.ShowConnectionMessageAsync(_device, "Выбор источника напряжения", $"Источник: {label}", true, 1, messageService);
      }
      catch (Exception ex)
      {
        await DeviceMessageBuilder.ShowConnectionMessageAsync(_device, "Ошибка выбора источника напряжения", ex.Message, false, 1, messageService);

        throw VoltageExceptionFactory.SetSourceFailed(label, ex.Message);
      }
    }

    public async Task SetVoltageLevelAsync(int integerPart, int decimalPart, IUserMessageService? messageService = null)
    {
      string value = $"{integerPart}.{decimalPart}";

      try
      {
        await _voltageManager.SetVoltageLevelAsync(integerPart, decimalPart);
        await DeviceMessageBuilder.ShowConnectionMessageAsync(_device, "Установка уровня напряжения", $"Напряжение: {value} В", true, 1, messageService);
      }
      catch (Exception ex)
      {
        await DeviceMessageBuilder.ShowConnectionMessageAsync(_device, "Ошибка установки напряжения", ex.Message, false, 1, messageService);

        throw VoltageExceptionFactory.SetLevelFailed(value, ex.Message);
      }
    }
  }
}
