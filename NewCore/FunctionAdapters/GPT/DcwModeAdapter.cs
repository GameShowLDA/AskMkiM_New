using AppConfiguration.Error.Device.Breakdown;
using DTO.Device.Breakdown.Capabilities;
using DTO.Device.Breakdown.Mode;
using DTO.Device.Breakdown.Model;
using DTO.Service;
using NewCore.Device;
using NewCore.Function.GPT;
using NewCore.Function.Helpers;
using Utilities;

namespace NewCore.FunctionAdapters.GPT
{
  /// <summary>
  /// Адаптер режима DCW для GPT-79904 с отображением сообщений.
  /// </summary>
  internal class DcwModeAdapter : IDcwModeBreakdown
  {
    private readonly GPT79904 _device;
    private readonly DcwMode _dcwMode;

    public IModeConfigurable Mode { get; set; }
    public IVoltageConfigurable Voltage { get; set; }
    public ICurrentLimitsConfigurable CurrentLimits { get; set; }
    public ITimeConfigurable Time { get; set; }
    public IOffsetConfigurable Offset { get; set; }
    public IArcCurrentConfigurable ArcCurrent { get; set; }
    public IMeasurable Measure { get; set; }
    public IConfigurationProvider<DcwConfiguration> Config { get; set; }

    public DcwModeAdapter(GPT79904 device)
    {
      _device = device ?? throw new ArgumentNullException(nameof(device));
      _dcwMode = new DcwMode(device);
      Voltage = _dcwMode.Voltage;
      Mode = _dcwMode.Mode;
      CurrentLimits = _dcwMode.CurrentLimits;
      Time = _dcwMode.Time;
      Offset = _dcwMode.Offset;
      ArcCurrent = _dcwMode.ArcCurrent;
      Measure = _dcwMode.Measure;
      Config = _dcwMode.Config;
    }

    #region Mode

    /// <inheritdoc />
    public async Task<(bool, string)> SetModeAsync(IUserMessageService? userMessageService = null)
    {
      var result = await _dcwMode.Mode.SetModeAsync();
      await DeviceMessageBuilder.ShowConnectionMessageAsync(_device, "Установка режима DCW", result.Success ? "DCW" : result.Message, result.Success, 1, userMessageService);

      if (!result.Success)
        throw DcwExceptionFactory.SetModeFailed(_device.Name, _device.NumberChassis, _device.Number, result.Message);

      return result;
    }

    /// <inheritdoc />
    public Task<(bool Success, string Message)> GetModeAsync() => _dcwMode.Mode.GetModeAsync();

    #endregion

    #region Voltage

    /// <inheritdoc />
    public async Task<(bool, string)> SetVoltageAsync(double value, IUserMessageService? userMessageService = null)
    {
      var result = await _dcwMode.Voltage.SetVoltageAsync(value);
      await DeviceMessageBuilder.ShowConnectionMessageAsync(_device, "Установка напряжения DCW", result.Success ? $"{value} В" : result.Message, result.Success, 1, userMessageService);

      if (!result.Success)
        throw DcwExceptionFactory.SetVoltageFailed(_device.Name, _device.NumberChassis, _device.Number, result.Message);

      return result;
    }

    /// <inheritdoc />
    public Task<double> GetVoltageAsync() => _dcwMode.Voltage.GetVoltageAsync();

    #endregion

    #region HighCurrentLimit

    /// <inheritdoc />
    public async Task<(bool, string)> SetHighCurrentLimitAsync(double value, IUserMessageService? userMessageService = null)
    {
      var result = await _dcwMode.CurrentLimits.SetHighCurrentLimitAsync(value);
      await DeviceMessageBuilder.ShowConnectionMessageAsync(_device, "Установка верхнего предела тока DCW", result.Success ? $"{value} мА" : result.Message, result.Success, 1, userMessageService);

      if (!result.Success)
        throw DcwExceptionFactory.SetHighLimitFailed(_device.Name, _device.NumberChassis, _device.Number, result.Message);

      return result;
    }

    /// <inheritdoc />
    public Task<double> GetHighCurrentLimitAsync() => _dcwMode.CurrentLimits.GetHighCurrentLimitAsync();

    #endregion

    #region LowCurrentLimit

    public async Task<(bool, string)> SetLowCurrentLimitAsync(double value, IUserMessageService? userMessageService = null)
    {
      var result = await _dcwMode.CurrentLimits.SetLowCurrentLimitAsync(value);
      await DeviceMessageBuilder.ShowConnectionMessageAsync(_device, "Установка нижнего предела тока DCW", result.Success ? $"{value} мА" : result.Message, result.Success, 1, userMessageService);

      if (!result.Success)
        throw DcwExceptionFactory.SetLowLimitFailed(_device.Name, _device.NumberChassis, _device.Number, result.Message);

      return result;
    }

    /// <inheritdoc />
    public Task<double> GetLowCurrentLimitAsync() => _dcwMode.CurrentLimits.GetLowCurrentLimitAsync();

    #endregion

    #region TestTime

    /// <inheritdoc />
    public async Task<(bool, string)> SetTestTimeAsync(double value, IUserMessageService? userMessageService = null)
    {
      var result = await _dcwMode.Time.SetTestTimeAsync(value);
      await DeviceMessageBuilder.ShowConnectionMessageAsync(_device, "Установка времени теста DCW", result.Success ? $"{value} сек" : result.Message, result.Success, 1, userMessageService);

      if (!result.Success)
        throw DcwExceptionFactory.SetTestTimeFailed(_device.Name, _device.NumberChassis, _device.Number, result.Message);

      return result;
    }

    /// <inheritdoc />
    public Task<double> GetTestTimeAsync() => _dcwMode.Time.GetTestTimeAsync();

    #endregion

    #region RampTime

    /// <inheritdoc />
    public async Task<(bool, string)> SetRampTimeAsync(double value, IUserMessageService? userMessageService = null)
    {
      var result = await _dcwMode.Time.SetRampTimeAsync(value);
      await DeviceMessageBuilder.ShowConnectionMessageAsync(_device, "Установка Ramp Time DCW", result.Success ? $"{value} сек" : result.Message, result.Success, 1, userMessageService);

      if (!result.Success)
        throw DcwExceptionFactory.SetRampTimeFailed(_device.Name, _device.NumberChassis, _device.Number, result.Message);

      return result;
    }

    /// <inheritdoc />
    public Task<double> GetRampTimeAsync() => _dcwMode.Time.GetRampTimeAsync();

    #endregion

    #region Offset
    /// <inheritdoc />
    public async Task<(bool, string)> SetOffsetAsync(double value, IUserMessageService? userMessageService = null)
    {
      var result = await UserActionHelper.GetRunWithUserRepeatAsync(() => _dcwMode.Offset.SetOffsetAsync(value, userMessageService), userMessageService);

      await DeviceMessageBuilder.ShowConnectionMessageAsync(_device, "Установка смещения DCW", result.Connect ? $"{value} мА" : result.Answer, result.Connect, 1, userMessageService);

      if (!result.Connect)
        throw DcwExceptionFactory.SetOffsetFailed(_device.Name, _device.NumberChassis, _device.Number, result.Answer);

      return result;
    }

    /// <inheritdoc />
    public Task<double> GetOffsetAsync() => _dcwMode.Offset.GetOffsetAsync();

    #endregion

    #region ArcCurrent
    /// <inheritdoc />
    public async Task<(bool, string)> SetArcCurrentAsync(double value, IUserMessageService? userMessageService = null)
    {
      var result = await _dcwMode.ArcCurrent.SetArcCurrentAsync(value);
      await DeviceMessageBuilder.ShowConnectionMessageAsync(_device, "Установка дугового тока DCW", result.Success ? $"{value} мА" : result.Message, result.Success, 1, userMessageService);

      if (!result.Success)
        throw DcwExceptionFactory.SetArcCurrentFailed(_device.Name, _device.NumberChassis, _device.Number, result.Message);

      return result;
    }

    /// <inheritdoc />
    public Task<double> GetArcCurrentAsync() => _dcwMode.ArcCurrent.GetArcCurrentAsync();

    #endregion

    #region Конфигурация и измерение

    /// <inheritdoc />
    public async Task<DcwConfiguration> ReadConfigurationAsync()
    {
      var config = await _dcwMode.Config.ReadConfigurationAsync();
      await DeviceMessageBuilder.ShowConnectionMessageAsync(_device, "Чтение конфигурации DCW", "Конфигурация считана", true, 1);
      return config;
    }

    /// <inheritdoc />
    public async Task<double> MeasureAsync(double param = 0, double rangeFrom = -1, double rangeTo = -1, IUserMessageService? userMessageService = null)
    {
      try
      {
        double result = await _dcwMode.Measure.MeasureAsync(param);
        await DeviceMessageBuilder.ShowConnectionMessageAsync(_device, "Измерение тока DCW", $"{result} мА", result >= 0, 2, userMessageService);
        return result;
      }
      catch (Exception ex)
      {
        await DeviceMessageBuilder.ShowConnectionMessageAsync(_device, "Ошибка измерения тока DCW", ex.Message, false, 2, userMessageService);
        return -1;
      }
    }

    /// <inheritdoc />
    public async Task ApplyVoltageAsync(IUserMessageService userMessageService = null)
    {
      await _dcwMode.Measure.ApplyVoltageAsync(userMessageService);
    }

    public async Task StopMeasure()
    {
      await _dcwMode.Measure.StopMeasure();
    }

    public void ResetConfiguration()
    {
      _dcwMode.Config.ResetConfiguration();
    }
    #endregion
  }
}
