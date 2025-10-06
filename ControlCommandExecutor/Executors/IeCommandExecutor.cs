using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using ControlCommandAnalyser.Model;
using ControlCommandAnalyser.Model.Chains;
using ControlCommandExecutor.BaseStrategies;
using ControlCommandExecutor.Execution;
using NewCore.Base.Interface.Main;
using Utilities;
using Utilities.Interface;
using Utilities.Models;
using Utilities.ResultProtocol;

namespace ControlCommandExecutor.Executors
{
  internal class IeCommandExecutor : ICommandExecutor
  {
    public string Mnemonic => "ИЕ";
    private double firstValue = 0;
    private double secondValue = 1000;

    public async Task ExecuteAsync(CommandExecutionContext context, ProtocolModel protocolModel)
    {
      if (!await AppConfiguration.Execution.ExecutionConfig.GetIsIdleModeEnabled())
      {
        await NewCore.Communication.DeviceCommandSender.ResetAllSystem();
      }

      var command = context.Command as IeCommandModel;
      context.TranslationControl.SetActiveLine(command.FormattedStartLineNumber);

      string nameCommand = $"{command.CommandNumber} {command.Mnemonic}";
      string message = string.Empty;

      foreach (var str in command.SourceLines)
      {
        message += "\r\n  " + str;
      }

      await context.Console.ShowMessageAsync(new ShowMessageModel($"\r\nВыполнение команды {nameCommand}", headerColor: ShowMessageModel.SuccessMessage.TitleColor, message: message, type: ShowMessageModel.MessageType.Command) { IndentLevel = 1 }, IsBlockStart: true);

      List<ShowMessageModel> errorMessage = new();

      var points = command.Scheme?.GroupModels?
            .SelectMany(chain => chain?.ChainModels ?? Enumerable.Empty<ChainModel>())
            .SelectMany(part => part?.PointModels ?? Enumerable.Empty<PointModel>())
            .ToList()
            ?? new List<PointModel>();

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
      await SettingFastMeter(meter, context.Console);

      if (command.LowerLimitCapacity.HasValue)
      {
        firstValue = command.LowerLimitCapacity.Value;
      }

      if (command.HigherLimitCapacity.HasValue)
      {
        secondValue = command.HigherLimitCapacity.Value;
      }

      BaseStrategies.ConnectedPointChecker.PerformMeasurementAsync measure = ResistanceMeasure;
      var errMes = await ConnectedPointChecker.CheckSequenceAsync(command.Scheme, measure, context.CommandExecutionManager, command, context.Console, (firstValue + secondValue) / 2);
      errorMessage.AddRange(errMes);

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

    /// <summary>
    /// Выполняет измерение между уже подключёнными точками.
    /// Предполагается, что коммутация завершена заранее.
    /// </summary>
    /// <returns>Задача, представляющая измерение.</returns>
    private async Task<(bool, string)> ResistanceMeasure(double value, IUserMessageService messageService, CancellationToken cancellationToken)
    {
      var meter = EquipmentService.GetFastMeterOrThrow(messageService);
      double answer = 0;

      var result = await UserActionHelper.GetRunWithUserRepeatAsync(async () =>
      {
        if (await AppConfiguration.Execution.ExecutionConfig.GetIsIdleModeEnabled() && await AppConfiguration.Execution.ExecutionConfig.GetIsErrorSimulationEnabled())
        {
          value = new Random().Next((int)(firstValue / 2), (int)(secondValue * 2));
        }
        answer = await meter.CapacitanceManager.MeasureCapacitanceAsync(value, userMessageService: messageService);
        var result = answer >= firstValue && answer <= secondValue;

        await messageService.ShowMessageAsync(new ShowMessageModel("Результат измерения ёмкости", message: $"{answer} пкФ", type: (answer >= firstValue && answer <= secondValue ? ShowMessageModel.MessageType.Success : ShowMessageModel.MessageType.Error)) { IndentLevel = 1 }, skipPause: true);
        await messageService.ShowMessageAsync(new ShowMessageModel("Диапазон допускаемых значений", message: $"от {firstValue} до {secondValue} пкФ") { IndentLevel = 2 }, skipPause: true);

        return result;
      }, messageService);

      return (result, answer + "пкФ");
    }

    private async Task SettingModuleRelayControl(List<IRelaySwitchModule> relaySwitchModules, IUserMessageService userMessageService)
    {
      foreach (var module in relaySwitchModules)
      {
        if (!await UserActionHelper.GetRunWithUserRepeatAsync(async () => (await module.ConnectableManager.InitializeAsync(userMessageService)).Connect, userMessageService))
        {
          throw AppConfiguration.Error.Device.ConnectionExceptionFactory.InitializeFailed(module.Name, module.NumberChassis, module.Number);
        }
        if (!await UserActionHelper.GetRunWithUserRepeatAsync(() => module.BusManager.ConnectBusAsync(NewCore.Enum.DeviceEnum.SwitchingBus.A1, userMessageService: userMessageService), userMessageService))
        {
          throw AppConfiguration.Error.Device.ModuleRelayControl.BusExceptionFactory.ConnectFailed(NewCore.Enum.DeviceEnum.SwitchingBus.A1.ToString(), module.Name, module.NumberChassis, module.Number);
        }
        if (!await UserActionHelper.GetRunWithUserRepeatAsync(() => module.BusManager.ConnectBusAsync(NewCore.Enum.DeviceEnum.SwitchingBus.B1, userMessageService: userMessageService), userMessageService))
        {
          throw AppConfiguration.Error.Device.ModuleRelayControl.BusExceptionFactory.ConnectFailed(NewCore.Enum.DeviceEnum.SwitchingBus.B1.ToString(), module.Name, module.NumberChassis, module.Number);
        }
      }
    }

    private async Task SettingsDeviceBusCommutatuion(ISwitchingDevice dbc, IUserMessageService userMessageService)
    {
      if (!await UserActionHelper.GetRunWithUserRepeatAsync(async () => (await dbc.ConnectableManager.InitializeAsync(userMessageService)).Connect, userMessageService))
      {
        throw AppConfiguration.Error.Device.ConnectionExceptionFactory.InitializeFailed(dbc.Name, dbc.NumberChassis, dbc.Number);
      }
      if (!await UserActionHelper.GetRunWithUserRepeatAsync(() => dbc.ConnectorManager.ConnectMultimeter(NewCore.Enum.DeviceEnum.SwitchingBusNew.AB1, userMessageService), userMessageService))
      {
        throw AppConfiguration.Error.Device.DeviceBusCommutation.ConnectorExceptionFactory.ConnectMultiMeterFailed(dbc.Name, dbc.NumberChassis, dbc.Number);
      }
    }

    private async Task SettingFastMeter(IFastMeter meter, IUserMessageService userMessageService)
    {
      string name = meter.Name;
      int numberChassis = meter.NumberChassis;
      int number = meter.Number;

      if (!await UserActionHelper.GetRunWithUserRepeatAsync(async () => (await meter.ConnectableManager.InitializeAsync(userMessageService)).Connect, userMessageService))
      {
        throw AppConfiguration.Error.Device.ConnectionExceptionFactory.ConnectFailed(name, numberChassis, number);
      }

      if (!await UserActionHelper.GetRunWithUserRepeatAsync(async () => (await meter.CapacitanceManager.SetCapacitanceModeAsync(userMessageService)), userMessageService))
      {
        throw AppConfiguration.Error.Device.Multimeter.CapacitanceExceptionFactory.SetModeFailed(name, numberChassis, number);
      }
    }
  }
}
