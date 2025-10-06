using NewCore.Base.Function.Breakdown;
using NewCore.Base.Function.Breakdown.Capabilities;
using NewCore.Device;
using NewCore.Function.GPT;
using NewCore.Function.GPT.Data;
using NewCore.Function.Helpers;
using Utilities.Interface;

namespace NewCore.FunctionAdapters.GPT
{
  /// <summary>
  /// Адаптер режима ACW для устройства GPT-79904 с отображением сообщений.
  /// </summary>
  internal class AcwModeAdapter : IAcwModeBreakdown
  {
    private readonly GPT79904 _device;
    private readonly AcwMode _acwMode;

    public IModeConfigurable Mode { get; set; }
    public IVoltageConfigurable Voltage { get; set; }
    public ICurrentLimitsConfigurable CurrentLimits { get; set; }
    public ITimeConfigurable Time { get; set; }

    public IOffsetConfigurable Offset { get; set; }
    public IArcCurrentConfigurable ArcCurrent { get; set; }
    public IFrequencyConfigurable FrequencyConfigurable { get; set; }
    public IMeasurable Measure { get; set; }
    public IConfigurationProvider<AcwConfiguration> Config { get ; set ; }

    public AcwModeAdapter(GPT79904 device)
    {
      _device = device ?? throw new ArgumentNullException(nameof(device));
      _acwMode = new AcwMode(device);
      Voltage = _acwMode.Voltage;
      Mode = _acwMode.Mode;
      CurrentLimits = _acwMode.CurrentLimits;
      Time = _acwMode.Time;
      Offset = _acwMode.Offset;
      ArcCurrent = _acwMode.ArcCurrent;
      FrequencyConfigurable = _acwMode.FrequencyConfigurable;
      Measure = _acwMode.Measure;
      Config = _acwMode.Config;
    }

    #region Mode

    /// <inheritdoc />
    public async Task<(bool, string)> SetModeAsync(IUserMessageService? userMessageService = null)
    {
      var result = await _acwMode.Mode.SetModeAsync();
      await DeviceMessageBuilder.ShowConnectionMessageAsync(_device, "Установка режима ACW", result.Success ? "ACW" : result.Message, result.Success, 1, userMessageService);

      if (!result.Success)
        throw new Exception($"Ошибка при установке режима ACW: {result.Message}");

      return result;
    }

    /// <inheritdoc />
    public Task<(bool Success, string Message)> GetModeAsync() => _acwMode.Mode.GetModeAsync();

    #endregion

    #region Voltage

    /// <inheritdoc />
    public async Task<(bool, string)> SetVoltageAsync(double value, IUserMessageService? userMessageService = null)
    {
      var result = await _acwMode.Voltage.SetVoltageAsync(value);
      await DeviceMessageBuilder.ShowConnectionMessageAsync(_device, "Установка напряжения ACW", result.Success ? $"{value} В" : result.Message, result.Success, 1, userMessageService);

      if (!result.Success)
      {
        return (false, $"Ошибка при установке напряжения ACW: {result.Message}");
      }

      return result;
    }

    /// <inheritdoc />
    public Task<double> GetVoltageAsync() => _acwMode.Voltage.GetVoltageAsync();

    #endregion

    #region HighCurrentLimit

    /// <inheritdoc />
    public async Task<(bool, string)> SetHighCurrentLimitAsync(double value, IUserMessageService? userMessageService = null)
    {
      var result = await _acwMode.CurrentLimits.SetHighCurrentLimitAsync(value);
      await DeviceMessageBuilder.ShowConnectionMessageAsync(_device, "Установка верхнего предела тока ACW", result.Success ? $"{value} мА" : result.Message, result.Success, 1, userMessageService);

      if (!result.Success)
        throw new Exception($"Ошибка при установке верхнего предела тока ACW: {result.Message}");

      return result;
    }

    /// <inheritdoc />
    public Task<double> GetHighCurrentLimitAsync() => _acwMode.CurrentLimits.GetHighCurrentLimitAsync();

    #endregion

    #region LowCurrentLimit

    /// <inheritdoc />
    public async Task<(bool, string)> SetLowCurrentLimitAsync(double value, IUserMessageService? userMessageService = null)
    {
      var result = await _acwMode.CurrentLimits.SetLowCurrentLimitAsync(value);
      await DeviceMessageBuilder.ShowConnectionMessageAsync(_device, "Установка нижнего предела тока ACW", result.Success ? $"{value} мА" : result.Message, result.Success, 1, userMessageService);

      if (!result.Success)
        throw new Exception($"Ошибка при установке нижнего предела тока ACW: {result.Message}");

      return result;
    }

    /// <inheritdoc />
    public Task<double> GetLowCurrentLimitAsync() => _acwMode.CurrentLimits.GetLowCurrentLimitAsync();

    #endregion

    #region TestTime

    /// <inheritdoc />
    public async Task<(bool, string)> SetTestTimeAsync(double value, IUserMessageService? userMessageService = null)
    {
      var result = await _acwMode.Time.SetTestTimeAsync(value);
      await DeviceMessageBuilder.ShowConnectionMessageAsync(_device, "Установка времени теста ACW", result.Success ? $"{value} сек" : result.Message, result.Success, 1, userMessageService);

      if (!result.Success)
        throw new Exception($"Ошибка при установке времени теста ACW: {result.Message}");

      return result;
    }

    /// <inheritdoc />
    public Task<double> GetTestTimeAsync() => _acwMode.Time.GetTestTimeAsync();

    #endregion

    #region RampTime

    /// <inheritdoc />
    public async Task<(bool, string)> SetRampTimeAsync(double value, IUserMessageService? userMessageService = null)
    {
      var result = await _acwMode.Time.SetRampTimeAsync(value);
      await DeviceMessageBuilder.ShowConnectionMessageAsync(_device, "Установка Ramp Time ACW", result.Success ? $"{value} сек" : result.Message, result.Success, 1, userMessageService);

      if (!result.Success)
        throw new Exception($"Ошибка при установке Ramp Time ACW: {result.Message}");

      return result;
    }

    /// <inheritdoc />
    public Task<double> GetRampTimeAsync() => _acwMode.Time.GetRampTimeAsync();

    #endregion

    #region Frequency

    /// <inheritdoc />
    public async Task<(bool, string)> SetFrequencyAsync(int frequency, IUserMessageService? userMessageService = null)
    {
      var result = await _acwMode.FrequencyConfigurable.SetFrequencyAsync(frequency);
      await DeviceMessageBuilder.ShowConnectionMessageAsync(_device, "Установка частоты ACW", result.Success ? $"{frequency} Гц" : result.Message, result.Success, 1, userMessageService);

      if (!result.Success)
        throw new Exception($"Ошибка при установке частоты ACW: {result.Message}");

      return result;
    }

    /// <inheritdoc />
    public Task<int> GetFrequencyAsync() => _acwMode.FrequencyConfigurable.GetFrequencyAsync();

    #endregion

    #region Offset

    /// <inheritdoc />
    public async Task<(bool, string)> SetOffsetAsync(double value, IUserMessageService? userMessageService = null)
    {
      var result = await _acwMode.Offset.SetOffsetAsync(value);
      await DeviceMessageBuilder.ShowConnectionMessageAsync(_device, "Установка смещения ACW", result.Success ? $"{value} мА" : result.Message, result.Success, 1, userMessageService);

      if (!result.Success)
        throw new Exception($"Ошибка при установке смещения ACW: {result.Message}");

      return result;
    }

    /// <inheritdoc />
    public Task<double> GetOffsetAsync() => _acwMode.Offset.GetOffsetAsync();

    #endregion

    #region ArcCurrent

    /// <inheritdoc />
    public async Task<(bool, string)> SetArcCurrentAsync(double value, IUserMessageService? userMessageService = null)
    {
      var result = await _acwMode.ArcCurrent.SetArcCurrentAsync(value);
      await DeviceMessageBuilder.ShowConnectionMessageAsync(_device, "Установка дугового тока ACW", result.Success ? $"{value} мА" : result.Message, result.Success, 1, userMessageService);

      if (!result.Success)
        throw new Exception($"Ошибка при установке дугового тока ACW: {result.Message}");

      return result;
    }

    /// <inheritdoc />
    public Task<double> GetArcCurrentAsync() => _acwMode.ArcCurrent.GetArcCurrentAsync();

    #endregion

    #region Конфигурация и измерения

    /// <inheritdoc />
    public async Task<AcwConfiguration> ReadConfigurationAsync()
    {
      var config = await _acwMode.Config.ReadConfigurationAsync();
      await DeviceMessageBuilder.ShowConnectionMessageAsync(_device, "Чтение конфигурации ACW", "Конфигурация считана", true, 1);
      return config;
    }

    /// <inheritdoc />
    public async Task<double> MeasureAsync(double param = 0, double rangeFrom = -1, double rangeTo = -1, IUserMessageService? userMessageService = null)
    {
      try
      {
        double result = await _acwMode.Measure.MeasureAsync(param, rangeFrom, rangeTo);
        await DeviceMessageBuilder.ShowConnectionMessageAsync(_device, "Измерение тока ACW", $"{result} мА", result >= 0, 2, userMessageService);
        return result;
      }
      catch (Exception ex)
      {
        await DeviceMessageBuilder.ShowConnectionMessageAsync(_device, "Ошибка измерения тока ACW", ex.Message, false, 2, userMessageService);
        throw new Exception($"Ошибка при измерении тока ACW: {ex.Message}");
      }
    }

    /// <inheritdoc />
    public async Task ApplyVoltageAsync(IUserMessageService userMessageService = null)
    {
      await _acwMode.Measure.ApplyVoltageAsync(userMessageService);
    }

    public async Task StopMeasure()
    {
      await _acwMode.Measure.StopMeasure();
    }

    public void ResetConfiguration()
    {
      _acwMode.Config.ResetConfiguration();
    }
    #endregion
  }
}
