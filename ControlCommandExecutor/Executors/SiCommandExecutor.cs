using ControlCommandAnalyser.Model;
using ControlCommandAnalyser.Model.Chains;
using ControlCommandExecutor.BaseStrategies.Data;
using ControlCommandExecutor.Execution;
using ControlCommandExecutor.Executors.Interface;
using DTO.Base.Models;
using DTO.Device.Breakdown;
using DTO.Device.RelaySwitchModule;
using DTO.Device.RelaySwitchModule.Model;
using DTO.Device.SwitchingDevice;
using DTO.Service;
using Errors.Device;
using Errors.Device.Adapters;
using Errors.Device.Breakdown;
using Errors.Device.DeviceBusCommutation;
using Errors.Device.ModuleRelayControl;
using Utilities;
using static DTO.Enum.DeviceEnums;

namespace ControlCommandExecutor.Executors
{
  internal class SiCommandExecutor : ICommandExecutor
  {
    public string Mnemonic => Utilities.EnumExtensions.GetDisplayInfo(DTO.Enum.Measurement.MeasurementTypeCommand.CI).DisplayName;

    public async Task ExecuteAsync(CommandExecutionContext context, ProtocolModel protocolModel)
    {
      var command = context.Command as SiCommandModel;
      context.TranslationControl.SetActiveLine(command.FormattedStartLineNumber);

      string nameCommand = $"{command.CommandNumber} {command.Mnemonic}";

      string message = string.Empty;

      foreach (var str in command.SourceLines)
      {
        message += "\r\n  " + str;
      }

      if (!string.IsNullOrEmpty(message))
      {
        await context.Console.ShowMessageAsync(new ShowMessageModel($"\r\nВыполнение команды {nameCommand}", headerColor: ShowMessageModel.SuccessMessage.TitleColor, message: message, type: ShowMessageModel.MessageType.Command) { IndentLevel = 1 }, IsBlockStart: true);
      }

      //var points = (List<PointModel>)(command.Points.Select(x => PointModel.ConvertToPointModels(x.Points)).Where(x => x != null));
      var points = command.Scheme?.GroupModels?
                  .SelectMany(chain => chain?.ChainModels ?? Enumerable.Empty<ChainModel>())
                  .SelectMany(part => part?.PointModels ?? Enumerable.Empty<PointModel>())
                  .ToList()
                  ?? new List<PointModel>();
      //var points = PointModel.ConvertToPointModels(command.Points);
      await EquipmentService.ValidatePointsExistInAnalyzedPointsAsync(points, context.Console);

      await context.Console.ShowMessageAsync(new ShowMessageModel($"Подготовка устройств"));

      var modules = points
          .Select(EquipmentService.GetModuleByPoint)
          .Where(m => m != null)
          .DistinctBy(m => (m.NumberChassis, m.Number))
          .ToList();

      await SettingModuleRelayControl(modules, context.Console);

      var dbc = EquipmentService.GetSwitchingDevice();

      await SettingsDeviceBusCommutatuion(dbc, context.Console);

      var breakDown = await EquipmentService.GetBreakdownTesterOrThrow(context.Console);
      await SettingBreakdown(breakDown, context.Console, command.Time.Value, command.Resistance.Value, command.Voltage.Value);

      List<ShowMessageModel> errorMessage = new();


      NodeFullContext methodExecutionContext = new NodeFullContext();
      methodExecutionContext.SchemeModel = command.Scheme;
      methodExecutionContext.CommandManager = context.CommandExecutionManager;
      methodExecutionContext.CommandModel = command;
      methodExecutionContext.MessageService = context.Console;
      methodExecutionContext.Resistance = command.Resistance.Value;
      methodExecutionContext.LowerLimit = 0;
      methodExecutionContext.HigherLimit = 80;
      methodExecutionContext.Unit = "МОм";
      methodExecutionContext.UnitMnemonic = "R";

      if (command.AlgorithmKey.Contains("К"))
      {
        BaseStrategies.NodeFullChecker.PerformMeasurementAsync measure = NodeFullPerformMeasurementAsync;
        methodExecutionContext.PerformMeasurementAsync = measure;
        var errMes = await BaseStrategies.NodeFullChecker.CheckSequenceAsync(methodExecutionContext);
        errorMessage.AddRange(errMes);
      }
      else if (command.AlgorithmKey.Contains("Г"))
      {
        BaseStrategies.NodeFullChecker.PerformMeasurementAsync measure = NodeFullPerformMeasurementAsync;
        var errMes = await BaseStrategies.MethodExecutor.CheckSequenceAsync(command.Scheme, measure, context.CommandExecutionManager, command, context.Console, command.Resistance.Value);
        errorMessage.AddRange(errMes);
      }
      else if (command.AlgorithmKey.Contains("Т1"))
      {
        BaseStrategies.NodeAccumulationChecker.PerformMeasurementAsync measure = NodeAccumulationPerformMeasurementAsync;
        var errMes = await BaseStrategies.PairwiseFirstPointChecker.CheckSequenceAsync(command.Scheme, measure, context.CommandExecutionManager, command, context.Console, command.Resistance.Value);
        errorMessage.AddRange(errMes);
      }
      else
      {
        BaseStrategies.NodeAccumulationChecker.PerformMeasurementAsync measure = NodeAccumulationPerformMeasurementAsync;
        var errMes = await BaseStrategies.NodeAccumulationChecker.CheckSequenceAsync(command.Scheme, context.CommandExecutionManager, command, measure, context.Console, context.Console.GetCancellationToken(), command.Resistance.Value);
        errorMessage.AddRange(errMes);
      }
      await ControlCommandAnalyser.PointFormater.MessageResult(errorMessage, context.Console);


      await context.Console.ShowMessageAsync(new ShowMessageModel("Сброс точек") { IndentLevel = 1 });
      foreach (var item in modules)
      {
        await item.PointManager.DisconnectingAllPoint(context.Console);
      }
      if (errorMessage.Count > 0)
      {
        if (!string.IsNullOrEmpty(message))
        {
          if (protocolModel.Errors.Keys.Contains(nameCommand + " " + 1))
          {
            protocolModel.Errors.Add(nameCommand + " " + 2, errorMessage);
          }
          else if (protocolModel.Errors.Keys.Contains(nameCommand + " " + 2))
          {
            protocolModel.Errors.Add(nameCommand + " " + 1, errorMessage);
          }
          else
          { 
            protocolModel.Errors.Add(nameCommand, errorMessage);
          }
        }
      }
    }

    private async Task SettingsDeviceBusCommutatuion(ISwitchingDevice dbc, IUserMessageService userMessageService)
    {
      if (!await UserActionHelper.GetRunWithUserRepeatAsync(async () => (await dbc.ConnectableManager.InitializeAsync(userMessageService)).Connect, userMessageService))
      {
        throw ConnectionExceptionAdapter.InitializeFailed(dbc.Name, dbc.NumberChassis, dbc.Number);
      }
      if (!await UserActionHelper.GetRunWithUserRepeatAsync(() => dbc.ConnectorManager.ConnectBreakdownTester(userMessageService), userMessageService))
      {
        throw ConnectorExceptionFactory.ConnectBreakdownFailed(dbc.Name, dbc.NumberChassis, dbc.Number);
      }
    }
    private async Task SettingModuleRelayControl(List<IRelaySwitchModule> relaySwitchModules, IUserMessageService userMessageService)
    {
      foreach (var module in relaySwitchModules)
      {
        if (!await UserActionHelper.GetRunWithUserRepeatAsync(async () => (await module.ConnectableManager.InitializeAsync(userMessageService)).Connect, userMessageService))
        {
          throw ConnectionExceptionAdapter.InitializeFailed(module.Name, module.NumberChassis, module.Number);
        }
        if (!await UserActionHelper.GetRunWithUserRepeatAsync(() => module.BusManager.ConnectBusAsync(SwitchingBus.A1, userMessageService: userMessageService), userMessageService))
        {
          throw BusExceptionFactory.ConnectFailed(SwitchingBus.A1.ToString(), module.Name, module.NumberChassis, module.Number);
        }
        if (!await UserActionHelper.GetRunWithUserRepeatAsync(() => module.BusManager.ConnectBusAsync(SwitchingBus.B1, userMessageService: userMessageService), userMessageService))
        {
          throw BusExceptionFactory.ConnectFailed(SwitchingBus.B1.ToString(), module.Name, module.NumberChassis, module.Number);
        }
      }
    }

    private async Task SettingBreakdown(IBreakdownTester breakDown, IUserMessageService userMessageService, double time, double resistance, double voltage)
    {
      string name = breakDown.Name;
      int numberChassis = breakDown.NumberChassis;
      int number = breakDown.Number;

      await userMessageService.ShowMessageAsync(new ShowMessageModel("Настройка пробойной установки"));
      if (!await UserActionHelper.GetRunWithUserRepeatAsync(async () => (await breakDown.ConnectableManager.InitializeAsync(userMessageService)).Connect, userMessageService))
      {
        throw ConnectionExceptionAdapter.ConnectFailed(name, numberChassis, number);
      }

      if (!await UserActionHelper.GetRunWithUserRepeatAsync(async () => (await breakDown.IrManger.Mode.SetModeAsync(userMessageService)).Success, userMessageService))
      {
        throw IrExceptionFactory.SetModeFailed(name, numberChassis, number);
      }

      if (!await UserActionHelper.GetRunWithUserRepeatAsync(async () => (await breakDown.IrManger.Time.SetTestTimeAsync(time, userMessageService)).Success, userMessageService))
      {
        throw IrExceptionFactory.SetTestTimeFailed(name, numberChassis, number);
      }

      if (!await UserActionHelper.GetRunWithUserRepeatAsync(async () => (await breakDown.IrManger.ResistanceLimits.SetLowResistanceLimitAsync(resistance, userMessageService)).Success, userMessageService))
      {
        throw IrExceptionFactory.SetLowLimitFailed(name, numberChassis, number);
      }

      if (!await UserActionHelper.GetRunWithUserRepeatAsync(async () => (await breakDown.IrManger.Voltage.SetVoltageAsync(voltage, userMessageService)).Success, userMessageService))
      {
        throw IrExceptionFactory.SetVoltageFailed(name, numberChassis, number);
      }
    }

    /// <summary>
    /// Выполняет измерение между уже подключёнными точками.
    /// Предполагается, что коммутация завершена заранее.
    /// </summary>
    /// <returns>Задача, представляющая измерение.</returns>
    private static async Task<(bool, string)> NodeAccumulationPerformMeasurementAsync(double value, IUserMessageService messageService, CancellationToken cancellationToken, VoltageEnum.Type typeVoltage = VoltageEnum.Type.ACW)
    {
      var breadDown = await EquipmentService.GetBreakdownTesterOrThrow(messageService);

      var result = await UserActionHelper.GetRunWithUserRepeatAsync(async () =>
      {
        var answer = await breadDown.IrManger.Measure.MeasureAsync(value, userMessageService: messageService);
        var result = !await AppConfiguration.Execution.ExecutionConfig.GetIsIdleModeEnabled() ? answer >= value : !await AppConfiguration.Execution.ExecutionConfig.GetIsErrorSimulationEnabled();

        if (!result || await AppConfiguration.DeviceDisplay.DeviceDisplayConfig.GetMeasurementResultsVisibilityAsync())
        {
          await messageService.ShowMessageAsync(new ShowMessageModel("Результат измерения сопротивления изоляции", message: $"{answer} МОм", type: (result ? ShowMessageModel.MessageType.Success : ShowMessageModel.MessageType.Error)) { IndentLevel = 1 }, skipPause: true);
        }
        return (result, answer.ToString());
      }, messageService);

      return result;
    }

    /// <summary>
    /// Выполняет измерение между уже подключёнными точками.
    /// Предполагается, что коммутация завершена заранее.
    /// </summary>
    /// <returns>Задача, представляющая измерение.</returns>
    private static async Task<(bool, double)> NodeFullPerformMeasurementAsync(double value, IUserMessageService messageService, CancellationToken cancellationToken, VoltageEnum.Type typeVoltage = VoltageEnum.Type.ACW)
    {
      var breadDown = await EquipmentService.GetBreakdownTesterOrThrow(messageService);
      double answer = -1;
      var result = await UserActionHelper.GetRunWithUserRepeatAsync(async () =>
      {
        messageService.GetCancellationToken().ThrowIfCancellationRequested();

        await messageService.ShowMessageAsync(new ShowMessageModel("Измерение сопротивления изоляции"));

        if (!await AppConfiguration.Execution.ExecutionConfig.GetIsIdleModeEnabled())
        {
          answer = await breadDown.IrManger.Measure.MeasureAsync(value, value, 60000, messageService);
        }
        else
        {
          answer = await AppConfiguration.Execution.ExecutionConfig.GetIsErrorSimulationEnabled() ? new Random().Next((int)value / 2, (int)value * 2) : value;
        }

          var type = ShowMessageModel.MessageType.Success;
        if (answer < value)
        {
          type = ShowMessageModel.MessageType.Error;
        }

        return type == ShowMessageModel.MessageType.Success ? true : false;
      }, messageService);

      return (result, answer);
    }
  }
}
