using System.Globalization;
using System.Text.RegularExpressions;
using DTO.Device.Breakdown;
using NewCore.Function.GPT.Command;
using DTO.Service;
using static AppConfiguration.Execution.ExecutionConfig;
using static NewCore.Function.GPT.Command.FunctionCommandManager;
using static Utilities.LoggerUtility;

namespace NewCore.Function.GPT.Helper
{
  static internal class MeasureHelper
  {
    /// <summary>
    /// Выполняет измерение.
    /// </summary>
    static public async Task<double> MeasureAsync(
      IBreakdownTester breakDown,
      double time,
      double timeRamp,
      int delayBeforeCall,
      double param = 0,
      double rangeFrom = -1,
      double rangeTo = -1,
      IUserMessageService? userMessageService = null)
    {
      LogInformation($"Начало {nameof(MeasureAsync)}", isDeviceLog: true);

      try
      {
        if (await GetIsIdleModeEnabled())
        {
          LogInformation($"{nameof(MeasureAsync)}: Устройство в Idle Mode. Возвращаем param.", isDeviceLog: true);
          return param;
        }

        var query = $"{FunctionCommandManager.GetCommandSyntax(FunctionCommand.FUNCTION_TEST)} ON";
        var timeDelay = Convert.ToInt32(timeRamp + time) * 1000;

        await breakDown.DeviceProtocol.QueryAsync(query, responseDelay: timeDelay, delayBeforeCall: delayBeforeCall);
        query = $"{FunctionCommandManager.GetCommandSyntax(FunctionCommand.MEASURE)} ?";

        var answerDevice = await breakDown.DeviceProtocol.QueryAsync(query, timeout: 500, delayBeforeCall: delayBeforeCall);
        var result = answerDevice.Split(',');
        var measureResulte = result[3];

        LogInformation($"Результат измерения: {measureResulte}", isDeviceLog: true);

        Match match = Regex.Match(measureResulte, @"\d+(\.\d+)?");
        if (match.Success)
        {
          var finalResult = double.Parse(match.Value, CultureInfo.InvariantCulture);
          LogInformation($"{nameof(MeasureAsync)}: Возвращаем значение = {finalResult}", isDeviceLog: true);
          return finalResult;
        }

        throw new FormatException("Число не найдено в строке.");
      }
      catch (Exception ex)
      {
        LogException($"Ошибка в {nameof(MeasureAsync)}", ex, isDeviceLog: true);
        throw;
      }
    }

    /// <summary>
    /// Останавливает текущее измерение.
    /// </summary>
    static public async Task StopMeasure(IBreakdownTester breakDown)
    {
      string response = await breakDown.DeviceProtocol.QueryAsync($"{GetCommandSyntax(FunctionCommand.FUNCTION_TEST)} OFF");
      await breakDown.DeviceProtocol.QueryAsync(response);
    }
  }
}
