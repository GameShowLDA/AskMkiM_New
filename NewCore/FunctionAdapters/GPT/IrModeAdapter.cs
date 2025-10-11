using AppConfiguration.Error.Device.Breakdown;
using DTO.Device.Breakdown.Capabilities;
using DTO.Device.Breakdown.Mode;
using DTO.Device.Breakdown.Model;
using DTO.Service;
using NewCore.Device;
using NewCore.Function.GPT;
using NewCore.Function.Helpers;

namespace NewCore.FunctionAdapters.GPT
{
  /// <summary>
  /// Адаптер режима IR (сопротивление изоляции) для GPT-79904 с сообщениями.
  /// </summary>
  internal class IrModeAdapter : IIrModeBreakdown
  {
    private readonly GPT79904 _device;
    private readonly IrMode _irMode;

    public IModeConfigurable Mode { get; set; }
    public IVoltageConfigurable Voltage { get; set; }
    public ITimeConfigurable Time { get; set; }
    public IOffsetConfigurable Offset { get; set; }
    public IMeasurable Measure { get; set; }
    public IConfigurationProvider<IrConfiguration> Config { get; set; }
    public IResistanceLimitsConfigurable ResistanceLimits { get; set; }

    public IrModeAdapter(GPT79904 device)
    {
      _device = device ?? throw new ArgumentNullException(nameof(device));
      _irMode = new IrMode(device);
      Mode = _irMode.Mode;
      Voltage = _irMode.Voltage;
      Time = _irMode.Time;
      Offset = _irMode.Offset;
      Measure = _irMode.Measure;
      Config = _irMode.Config;
      ResistanceLimits = _irMode.ResistanceLimits;
    }

    #region Mode

    public async Task<(bool, string)> SetModeAsync(IUserMessageService? userMessageService = null)
    {
      var result = await _irMode.Mode.SetModeAsync();
      await DeviceMessageBuilder.ShowConnectionMessageAsync(_device, "Установка режима IR", result.Success ? "IR" : result.Message, result.Success, 1, userMessageService);

      if (!result.Success)
        throw IrExceptionFactory.SetModeFailed(_device.Name, _device.NumberChassis, _device.Number, result.Message);

      return result;
    }


    public Task<(bool Success, string Message)> GetModeAsync() => _irMode.Mode.GetModeAsync();

    #endregion

    #region Voltage

    public async Task<(bool, string)> SetVoltageAsync(double value, IUserMessageService? userMessageService = null)
    {
      var result = await _irMode.Voltage.SetVoltageAsync(value);
      await DeviceMessageBuilder.ShowConnectionMessageAsync(_device, "Установка напряжения IR", result.Success ? $"{value} В" : result.Message, result.Success, 1, userMessageService);

      if (!result.Success)
        throw IrExceptionFactory.SetVoltageFailed(_device.Name, _device.NumberChassis, _device.Number);

      return result;
    }

    public async Task<double> GetVoltageAsync()
    {
      var value = await _irMode.Voltage.GetVoltageAsync();
      await DeviceMessageBuilder.ShowConnectionMessageAsync(_device, "Чтение напряжения IR", $"{value} В", value > 0, 1);
      return value;
    }

    #endregion

    #region HighResistanceLimit

    public async Task<(bool, string)> SetHighResistanceLimitAsync(double value, IUserMessageService? userMessageService = null)
    {
      var result = await _irMode.ResistanceLimits.SetHighResistanceLimitAsync(value);
      await DeviceMessageBuilder.ShowConnectionMessageAsync(_device, "Установка верхнего предела сопротивления IR", result.Success ? $"{value} ГОм" : result.Message, result.Success, 1, userMessageService);

      if (!result.Success)
        throw IrExceptionFactory.SetHighLimitFailed(_device.Name, _device.NumberChassis, _device.Number, result.Message);

      return result;
    }

    public Task<double> GetHighResistanceLimitAsync() => _irMode.ResistanceLimits.GetHighResistanceLimitAsync();

    #endregion

    #region LowResistanceLimit

    public async Task<(bool, string)> SetLowResistanceLimitAsync(double value, IUserMessageService? userMessageService = null)
    {
      var result = await _irMode.ResistanceLimits.SetLowResistanceLimitAsync(value);
      await DeviceMessageBuilder.ShowConnectionMessageAsync(_device, "Установка нижнего предела сопротивления IR", result.Success ? $"{value} МОм" : result.Message, result.Success, 1, userMessageService);

      if (!result.Success)
        throw IrExceptionFactory.SetLowLimitFailed(_device.Name, _device.NumberChassis, _device.Number, result.Message);

      return result;
    }

    public Task<double> GetLowResistanceLimitAsync() => _irMode.ResistanceLimits.GetLowResistanceLimitAsync();

    #endregion

    #region TestTime

    public async Task<(bool, string)> SetTestTimeAsync(double value, IUserMessageService? userMessageService = null)
    {
      var result = await _irMode.Time.SetTestTimeAsync(value);
      await DeviceMessageBuilder.ShowConnectionMessageAsync(_device, "Установка времени измерения IR", result.Success ? $"{value} сек" : result.Message, result.Success, 1, userMessageService);

      if (!result.Success)
        throw IrExceptionFactory.SetTestTimeFailed(_device.Name, _device.NumberChassis, _device.Number, result.Message);

      return result;
    }

    public Task<double> GetTestTimeAsync() => _irMode.Time.GetTestTimeAsync();

    #endregion

    #region RampTestTime
    public async Task<(bool Success, string Message)> SetRampTimeAsync(double value, IUserMessageService? userMessageService = null)
    {
      var result = await _irMode.Time.SetRampTimeAsync(value);
      await DeviceMessageBuilder.ShowConnectionMessageAsync(_device, "Установка времени нарастания IR", result.Success ? $"{value} сек" : result.Message, result.Success, 1, userMessageService);

      if (!result.Success)
        throw IrExceptionFactory.SetTestTimeFailed(_device.Name, _device.NumberChassis, _device.Number, result.Message);

      return result;
    }

    public Task<double> GetRampTimeAsync() => _irMode.Time.GetRampTimeAsync();

    #endregion
    #region Offset

    public async Task<(bool, string)> SetOffsetAsync(double value, IUserMessageService? userMessageService = null)
    {
      var result = await _irMode.Offset.SetOffsetAsync(value);
      await DeviceMessageBuilder.ShowConnectionMessageAsync(_device, "Установка смещения IR", result.Success ? $"{value} ГОм" : result.Message, result.Success, 1, userMessageService);

      if (!result.Success)
        throw IrExceptionFactory.SetOffsetFailed(_device.Name, _device.NumberChassis, _device.Number, result.Message);

      return result;
    }

    public Task<double> GetOffsetAsync() => _irMode.Offset.GetOffsetAsync();

    #endregion

    #region Измерение и конфигурация
    public async Task StopMeasure()
    {
      await _irMode.Measure.StopMeasure();
    }

    public async Task<double> MeasureAsync(double param = 0, double rangeFrom = -1, double rangeTo = 60000, IUserMessageService? userMessageService = null)
    {
      if (rangeTo == -1)
      {
        rangeTo = 60000;
      }
      try
      {
        double result = await _irMode.Measure.MeasureAsync(param, rangeFrom, rangeTo);
        await DeviceMessageBuilder.ShowConnectionMessageAsync(_device, "Измерение сопротивления изоляции", $"{result} МОм", result >= rangeFrom && result <= rangeTo, 2, userMessageService);
        return result;
      }
      catch (Exception ex)
      {
        await DeviceMessageBuilder.ShowConnectionMessageAsync(_device, "Ошибка измерения сопротивления изоляции", ex.Message, false, 2, userMessageService);
        return -1;
      }
    }

    public async Task<IrConfiguration> ReadConfigurationAsync()
    {
      var config = await _irMode.Config.ReadConfigurationAsync();
      await DeviceMessageBuilder.ShowConnectionMessageAsync(_device, "Чтение конфигурации IR", "Конфигурация считана", true, 1);
      return config;
    }
    public void ResetConfiguration()
    {
      _irMode.Config.ResetConfiguration();
    }

    public Task ApplyVoltageAsync(IUserMessageService? userMessageService = null)
    {
      throw new NotImplementedException();
    }
    #endregion
  }
}
