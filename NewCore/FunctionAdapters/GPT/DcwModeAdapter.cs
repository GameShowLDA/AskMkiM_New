using DTO.Device.Breakdown.Capabilities;
using DTO.Device.Breakdown.Mode;
using DTO.Device.Breakdown.Model;
using DTO.Service;
using Errors.Device.Breakdown;
using NewCore.Device;
using NewCore.Function.GPT;
using NewCore.Function.Helpers;
using Utilities;
using static NewCore.FunctionAdapters.GPT.AcwModeAdapter;

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
      Mode = new DcwAdapterMode(_dcwMode, _device);
      Voltage = new VoltageAdapterMode(_dcwMode, _device);
      CurrentLimits = new CurrentLimitsAdapterMode(_dcwMode, _device);
      Time = new TimeAdapterMode(_dcwMode, _device);
      Offset = new OffsetAdapterMode(_dcwMode, _device);
      ArcCurrent = new ArcCurrentAdapterMode(_dcwMode, _device);
      Measure = new MeasureAdapterMode(_dcwMode, _device);
      Config = new ConfigAdapterMode(_dcwMode, device);
    }

    public class DcwAdapterMode : IModeConfigurable
    {
      DcwMode _dcwMode = null;
      GPT79904 _device = null;
      public async Task<(bool, string)> SetModeAsync(IUserMessageService? userMessageService = null)
      {
        var result = await _dcwMode.Mode.SetModeAsync();
        await DeviceMessageBuilder.ShowConnectionMessageAsync(_device, "Установка режима DCW", result.Success ? "DCW" : result.Message, result.Success, 1, userMessageService);

        if (!result.Success)
          throw DcwExceptionFactory.SetModeFailed(_device.Name, _device.NumberChassis, _device.Number, result.Message);

        return result;
      }
      public Task<(bool Success, string Message)> GetModeAsync() => _dcwMode.Mode.GetModeAsync();

      public DcwAdapterMode(DcwMode acwMode, GPT79904 device)
      {
        _dcwMode = acwMode;
        _device = device;
      }
    }

    public class VoltageAdapterMode : IVoltageConfigurable
    {
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

      DcwMode _dcwMode = null;
      GPT79904 _device = null;

      public VoltageAdapterMode(DcwMode acwMode, GPT79904 device)
      {
        _dcwMode = acwMode;
        _device = device;
      }
    }

    public class CurrentLimitsAdapterMode : ICurrentLimitsConfigurable
    {
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

      DcwMode _dcwMode = null;
      GPT79904 _device = null;

      public CurrentLimitsAdapterMode(DcwMode acwMode, GPT79904 device)
      {
        _dcwMode = acwMode;
        _device = device;
      }
    }

    public class TimeAdapterMode : ITimeConfigurable
    {
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

      DcwMode _dcwMode = null;
      GPT79904 _device = null;

      public TimeAdapterMode(DcwMode acwMode, GPT79904 device)
      {
        _dcwMode = acwMode;
        _device = device;
      }
    }

    public class OffsetAdapterMode : IOffsetConfigurable
    {
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

      DcwMode _dcwMode = null;
      GPT79904 _device = null;

      public OffsetAdapterMode(DcwMode acwMode, GPT79904 device)
      {
        _dcwMode = acwMode;
        _device = device;
      }
    }

    public class ArcCurrentAdapterMode : IArcCurrentConfigurable
    {
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

      DcwMode _dcwMode = null;
      GPT79904 _device = null;

      public ArcCurrentAdapterMode(DcwMode acwMode, GPT79904 device)
      {
        _dcwMode = acwMode;
        _device = device;
      }
    }

    public class MeasureAdapterMode : IMeasurable
    {
      public async Task ApplyVoltageAsync(IUserMessageService userMessageService = null)
      {
        await _dcwMode.Measure.ApplyVoltageAsync(userMessageService);
      }

      public async Task StopMeasure()
      {
        await _dcwMode.Measure.StopMeasure();
      }

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

      DcwMode _dcwMode = null;
      GPT79904 _device = null;

      public MeasureAdapterMode(DcwMode acwMode, GPT79904 device)
      {
        _dcwMode = acwMode;
        _device = device;
      }
    }

    public class ConfigAdapterMode : IConfigurationProvider<DcwConfiguration>
    {
      public async Task<DcwConfiguration> ReadConfigurationAsync()
      {
        var config = await _dcwMode.Config.ReadConfigurationAsync();
        await DeviceMessageBuilder.ShowConnectionMessageAsync(_device, "Чтение конфигурации DCW", "Конфигурация считана", true, 1);
        return config;
      }

      public void ResetConfiguration()
      {
        _dcwMode.Config.ResetConfiguration();
      }

      DcwMode _dcwMode = null;
      GPT79904 _device = null;

      public ConfigAdapterMode(DcwMode acwMode, GPT79904 device)
      {
        _dcwMode = acwMode;
        _device = device;
      }
    }

  }
}
