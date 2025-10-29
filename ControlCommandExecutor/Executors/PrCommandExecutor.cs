using ControlCommandAnalyser.Model;
using ControlCommandAnalyser.Model.Chains;
using ControlCommandExecutor.BaseStrategies;
using ControlCommandExecutor.Execution;
using DTO.Base.Models;
using DTO.Device.FastMeter;
using DTO.Device.RelaySwitchModule;
using DTO.Device.RelaySwitchModule.Model;
using DTO.Device.SwitchingDevice;
using DTO.Service;
using Errors.Device;
using Utilities;
using static DTO.Enum.DeviceEnums;

namespace ControlCommandExecutor.Executors
{
  internal class PrCommandExecutor : ICommandExecutor
  {
    public string Mnemonic => "ПР";
    static private PointModel _basePoint;
    private double firstValue = 0;
    private double secondValue = 100000;

    public async Task ExecuteAsync(CommandExecutionContext context, ProtocolModel protocolModel)
    {
      if (!await AppConfiguration.Execution.ExecutionConfig.GetIsIdleModeEnabled())
      {
        await NewCore.Communication.DeviceCommandSender.ResetAllSystem();
      }

      var command = context.Command as PrCommandModel;
      context.TranslationControl.SetActiveLine(command.FormattedStartLineNumber);

      string nameCommand = $"{command.CommandNumber} {command.Mnemonic}";
      string message = string.Empty;

      foreach (var str in command.SourceLines)
      {
        message += "\r\n  " + str;
      }

      await context.Console.ShowMessageAsync(new ShowMessageModel($"\r\nВыполнение команды {nameCommand}", headerColor: ShowMessageModel.SuccessMessage.TitleColor, message: message, type: ShowMessageModel.MessageType.Command) { IndentLevel = 1 }, IsBlockStart: true);

      //var points = (List<PointModel>)(command.Points.Select(x => PointModel.ConvertToPointModels(x.Points)).Where(x => x != null));
      var points = command.Scheme?.GroupModels?
                  .SelectMany(chain => chain?.ChainModels ?? Enumerable.Empty<ChainModel>())
                  .SelectMany(part => part?.PointModels ?? Enumerable.Empty<PointModel>())
                  .ToList()
                  ?? new List<PointModel>();

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

      var meter = EquipmentService.GetFastMeterOrThrow(context.Console);
      await SettingMeter(meter, context.Console);

      await context.Console.ShowMessageAsync(new ShowMessageModel($"Выполнение измерений"), IsBlockStart: true);

      double resistance = 0;
      if (command.LowerLimitResistance.HasValue)
      {
        firstValue = command.LowerLimitResistance.Value;
        resistance = command.LowerLimitResistance.Value;
      }
      else
      {
        firstValue = 0;
      }

      if (command.HigherLimitResistance.HasValue)
      {
        secondValue = command.HigherLimitResistance.Value;
      }
      else
      {
        secondValue = meter.MaxContinuityResistance;
      }


      List<ShowMessageModel> errorMessage = new();
      BaseStrategies.ConnectedPointChecker.PerformMeasurementAsync measurePointConnected = ConnectedPointCheckerMeasurementAsync;
      var connectErrMes = await ConnectedPointChecker.CheckSequenceAsync(command.Scheme, measurePointConnected, context.CommandExecutionManager, command, context.Console, resistance);
      errorMessage.AddRange(connectErrMes);

      if (command.AlgorithmKey.Contains("К"))
      {
        BaseStrategies.NodeFullChecker.PerformMeasurementAsync measure = NodeFullPerformMeasurementAsync;
        var errMes = await BaseStrategies.NodeFullChecker.CheckSequenceAsync(command.Scheme, measure, context.CommandExecutionManager, command, context.Console, resistance);
        errorMessage.AddRange(errMes);
      }
      else if (command.AlgorithmKey.Contains("Г"))
      {
        BaseStrategies.NodeFullChecker.PerformMeasurementAsync measure = NodeFullPerformMeasurementAsync;
        var errMes = await BaseStrategies.MethodExecutor.CheckSequenceAsync(command.Scheme, measure, context.CommandExecutionManager, command, context.Console, resistance);
        errorMessage.AddRange(errMes);
      }
      else if (command.AlgorithmKey.Contains("Т1"))
      {
        BaseStrategies.NodeAccumulationChecker.PerformMeasurementAsync measure = NodeAccumulationPerformMeasurementAsync;
        var errMes = await BaseStrategies.PairwiseFirstPointChecker.CheckSequenceAsync(command.Scheme, measure, context.CommandExecutionManager, command, context.Console, resistance);
        errorMessage.AddRange(errMes);
      }
      else
      {
        BaseStrategies.NodeAccumulationChecker.PerformMeasurementAsync measure = NodeAccumulationPerformMeasurementAsync;
        var errMes = await BaseStrategies.NodeAccumulationChecker.CheckSequenceAsync(command.Scheme, context.CommandExecutionManager, command, measure, context.Console, context.Console.GetCancellationToken(), resistance);
        errorMessage.AddRange(errMes);
      }

      await PointFormater.MessageResult(errorMessage, context.Console);

      await context.Console.ShowMessageAsync(new ShowMessageModel("Сброс точек") { IndentLevel = 1 });
      foreach (var item in modules)
      {
        await item.PointManager.DisconnectingAllPoint(context.Console);
      }

      if (errorMessage.Count > 0)
      {
        protocolModel.Errors.Add(nameCommand, errorMessage);
      }
    }


    private async Task SettingModuleRelayControl(List<IRelaySwitchModule> relaySwitchModules, IUserMessageService userMessageService)
    {
      foreach (var module in relaySwitchModules)
      {
        if (!await UserActionHelper.GetRunWithUserRepeatAsync(async () => (await module.ConnectableManager.InitializeAsync(userMessageService)).Connect, userMessageService))
        {
          throw ConnectionExceptionFactory.InitializeFailed(module.Name, module.NumberChassis, module.Number);
        }
        if (!await UserActionHelper.GetRunWithUserRepeatAsync(() => module.BusManager.ConnectBusAsync(SwitchingBus.A1, userMessageService: userMessageService), userMessageService))
        {
          throw AppConfiguration.Error.Device.ModuleRelayControl.BusExceptionFactory.ConnectFailed(SwitchingBus.A1.ToString(), module.Name, module.NumberChassis, module.Number);
        }
        if (!await UserActionHelper.GetRunWithUserRepeatAsync(() => module.BusManager.ConnectBusAsync(SwitchingBus.B1, userMessageService: userMessageService), userMessageService))
        {
          throw AppConfiguration.Error.Device.ModuleRelayControl.BusExceptionFactory.ConnectFailed(SwitchingBus.B1.ToString(), module.Name, module.NumberChassis, module.Number);
        }
      }
    }

    private async Task SettingsDeviceBusCommutatuion(ISwitchingDevice dbc, IUserMessageService userMessageService)
    {
      if (!await UserActionHelper.GetRunWithUserRepeatAsync(async () => (await dbc.ConnectableManager.InitializeAsync(userMessageService)).Connect, userMessageService))
      {
        throw ConnectionExceptionFactory.InitializeFailed(dbc.Name, dbc.NumberChassis, dbc.Number);
      }
      if (!await UserActionHelper.GetRunWithUserRepeatAsync(() => dbc.ConnectorManager.ConnectMultimeter(SwitchingBusNew.AB1, userMessageService), userMessageService))
      {
        throw AppConfiguration.Error.Device.DeviceBusCommutation.ConnectorExceptionFactory.ConnectMultiMeterFailed(dbc.Name, dbc.NumberChassis, dbc.Number);
      }
    }

    private async Task SettingMeter(IFastMeter meter, IUserMessageService userMessageService)
    {
      string name = meter.Name;
      int numberChassis = meter.NumberChassis;
      int number = meter.Number;

      await userMessageService.ShowMessageAsync(new ShowMessageModel("Настройка мультиметра"));
      if (!await UserActionHelper.GetRunWithUserRepeatAsync(async () => (await meter.ConnectableManager.ConnectAsync(userMessageService)).Connect, userMessageService))
      {
        throw AppConfiguration.Error.Device.Breakdown.IrExceptionFactory.SetVoltageFailed(name, numberChassis, number);
      }
      if (!await UserActionHelper.GetRunWithUserRepeatAsync(async () => (await meter.ContinuityManager.SetContinuityModeAsync(userMessageService)), userMessageService))
      {
        throw AppConfiguration.Error.Device.Breakdown.IrExceptionFactory.SetVoltageFailed(name, numberChassis, number);
      }
    }

    /// <summary>
    /// Выполняет измерение между уже подключёнными точками метод накапливающего узла.
    /// Предполагается, что коммутация завершена заранее.
    /// </summary>
    /// <returns>Задача, представляющая измерение.</returns>
    private async Task<bool> NodeAccumulationPerformMeasurementAsync(double resistance, IUserMessageService messageService, CancellationToken cancellationToken, VoltageEnum.Type type = VoltageEnum.Type.ACW)
    {
      var fastMeter = EquipmentService.GetFastMeterOrThrow(messageService);

      var result = await UserActionHelper.GetRunWithUserRepeatAsync(async () =>
      {
        var answer = await fastMeter.ContinuityManager.CheckContinuityAsync(resistance, messageService);
        var result = !await AppConfiguration.Execution.ExecutionConfig.GetIsIdleModeEnabled() ? answer > resistance : !await AppConfiguration.Execution.ExecutionConfig.GetIsErrorSimulationEnabled();

        await messageService.ShowMessageAsync(new ShowMessageModel("Результат измерения сопротивления", message: $"{answer} Ом", type: (answer >= firstValue && answer <= secondValue ? ShowMessageModel.MessageType.Success : ShowMessageModel.MessageType.Error)) { IndentLevel = 1 }, skipPause: true);
        await messageService.ShowMessageAsync(new ShowMessageModel("Диапазон допускаемых значений", message: $"от {firstValue} до {secondValue} Ом") { IndentLevel = 2 }, skipPause: true);
        return result;

      }, messageService);

      return result;
    }

    /// <summary>
    /// Выполняет измерение между уже подключёнными точками метод полного узла.
    /// Предполагается, что коммутация завершена заранее.
    /// </summary>
    /// <returns>Задача, представляющая измерение.</returns>
    private async Task<(bool, double)> NodeFullPerformMeasurementAsync(double resistance, IUserMessageService messageService, CancellationToken cancellationToken, VoltageEnum.Type type = VoltageEnum.Type.ACW)
    {
      var fastMeter = EquipmentService.GetFastMeterOrThrow(messageService);
      double answer = -1;
      var result = await UserActionHelper.GetRunWithUserRepeatAsync(async () =>
      {
        answer = await fastMeter.ContinuityManager.CheckContinuityAsync(resistance, messageService);
        var result = !await AppConfiguration.Execution.ExecutionConfig.GetIsIdleModeEnabled() ? answer > resistance : !await AppConfiguration.Execution.ExecutionConfig.GetIsErrorSimulationEnabled();

        await messageService.ShowMessageAsync(new ShowMessageModel("Результат измерения сопротивления", message: $"{answer} Ом", type: (answer >= firstValue && answer <= secondValue ? ShowMessageModel.MessageType.Success : ShowMessageModel.MessageType.Error)) { IndentLevel = 1 }, skipPause: true);
        await messageService.ShowMessageAsync(new ShowMessageModel("Диапазон допускаемых значений", message: $"от {firstValue} до {secondValue} Ом") { IndentLevel = 2 }, skipPause: true);

        return result;

      }, messageService);

      return (result, answer);
    }

    /// <summary>
    /// Выполняет измерение между уже подключёнными точками методом первой точки.
    /// Предполагается, что коммутация завершена заранее.
    /// </summary>
    /// <returns>Задача, представляющая измерение.</returns>
    private async Task<(bool, string)> ConnectedPointCheckerMeasurementAsync(double resistance, IUserMessageService messageService, CancellationToken cancellationToken)
    {
      var fastMeter = EquipmentService.GetFastMeterOrThrow(messageService);
      double answer = -1;

      var result = await UserActionHelper.GetRunWithUserRepeatAsync(async () =>
      {
        answer = await fastMeter.ContinuityManager.CheckContinuityAsync(resistance, messageService);
        var result = !await AppConfiguration.Execution.ExecutionConfig.GetIsIdleModeEnabled() ? answer < resistance : !await AppConfiguration.Execution.ExecutionConfig.GetIsErrorSimulationEnabled();

        await messageService.ShowMessageAsync(new ShowMessageModel("Результат измерения сопротивления", message: $"{answer} Ом", type: (answer >= firstValue && answer <= secondValue ? ShowMessageModel.MessageType.Success : ShowMessageModel.MessageType.Error)) { IndentLevel = 1 }, skipPause: true);
        await messageService.ShowMessageAsync(new ShowMessageModel("Диапазон допускаемых значений", message: $"от {firstValue} до {secondValue} Ом") { IndentLevel = 2 }, skipPause: true);

        return result;

      }, messageService);

      return (result, answer + "Ом");
    }
  }
}
