using ControlCommandAnalyser.Model;
using ControlCommandAnalyser.Model.Chains;
using ControlCommandExecutor.Execution;
using DTO.Base.Models;
using DTO.Base.Models.MeasurementError;
using DTO.Device.Base;
using DTO.Device.FastMeter;
using DTO.Device.RelaySwitchModule.Model;
using DTO.Enum;
using DTO.Service;
using Errors.Device.ModuleRelayControl;
using Errors.Translation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using static DTO.Enum.DeviceEnums;
using static DTO.Enum.Measurement;

namespace ControlCommandExecutor.BaseStrategies
{
  internal class PairwiseFirstPointCheckerAlt
  {
    /// <summary>
    /// Выполняет последовательную проверку точек относительно первой.
    /// </summary>
    /// <param name="points">Список точек для проверки.</param>
    /// <param name="messageService">Сервис отображения сообщений.</param>
    /// <returns>Задача, представляющая выполнение проверки.</returns>
    static public async Task<List<ShowMessageModel>> CheckSequenceAsync(SchemeModel schemeModel, CommandExecutionManager manager, BaseCommandModel baseCommandModel, IUserMessageService messageService, double resistance = 0)
    {
      List<ShowMessageModel> errorsMessgae = new List<ShowMessageModel>();

      List<List<ChainModel>> errorChain = new();
      var pointsListSource = schemeModel.GetPointsConnected();
      if (pointsListSource.Count == 0)
      {
        return errorsMessgae;
      }

      foreach (var groups in pointsListSource)
      {
        foreach (var chains in groups)
        {
          bool errorPoint = false;
          var str = string.Empty;

          foreach (var point in chains)
          {
            str += $"{(EquipmentService.GetPointKey(point))},";
          }
          str = str.Remove(str.Length - 1);
          await messageService.ShowMessageAsync(new ShowMessageModel($"Проверка {str}"), IsBlockStart: true);

          var _basePoint = chains.First();
          await ConnectToBusAAndBAsync(messageService, _basePoint);

          var Rt1 = await GetResistanceAsync(messageService, resistance);
          if (Rt1 > 100)
          {
            errorPoint = true;
            await messageService.ShowMessageAsync(new ShowMessageModel("Результат измерения сопротивления", message: $"Нет подлючения точки {_basePoint.Mnemonic}", type: ShowMessageModel.MessageType.Error) { IndentLevel = 1 });
          }
          else
          {
            await messageService.ShowMessageAsync(new ShowMessageModel("Результат измерения сопротивления", message: $"{Rt1:F5} Ом") { IndentLevel = 1 });
          }

          await DisconnectToBusBAsync(messageService, _basePoint);

          for (int i = 1; i < chains.Count; i++)
          {
            var point = chains[i];
            await ConnectToBusAAndBAsync(messageService, point);

            var Rt2 = await GetResistanceAsync(messageService, resistance);
            if (Rt1 > 100)
            {
              errorPoint = true;
              await messageService.ShowMessageAsync(new ShowMessageModel("Результат измерения сопротивления", message: $"Нет подлючения точки {point.Mnemonic}", type: ShowMessageModel.MessageType.Error) { IndentLevel = 1 });
            }
            else
            {
              await messageService.ShowMessageAsync(new ShowMessageModel("Результат измерения сопротивления", message: $"{Rt2:F5} Ом") { IndentLevel = 1 });
            }

            await DisconnectToBusAAsync(messageService, point);

            var Rt = await GetResistanceAsync(messageService, resistance);
            await messageService.ShowMessageAsync(new ShowMessageModel("Результат измерения сопротивления", message: $"{Rt:F5} Ом") { IndentLevel = 1 });
            await DisconnectToBusBAsync(messageService, point);

            await messageService.ShowMessageAsync(new ShowMessageModel("Итог измерений"));

            var LowerBound = (baseCommandModel as EhtCommandModel).LowerLimitResistance.Value;
            var UpperBound = (baseCommandModel as EhtCommandModel).HigherLimitResistance.Value;

            double Rx = 0;
            if (!errorPoint)
            {
              Rx = Rt - (Rt1 + Rt2) / 2;
            }
            else
            {
              Rx = Rt;
            }

            var result = !await AppConfiguration.Execution.ExecutionConfig.GetIsIdleModeEnabled() ? Rx : !await AppConfiguration.Execution.ExecutionConfig.GetIsErrorSimulationEnabled() ? (LowerBound + UpperBound) / 2 : Rx;

            if (result < 0)
            {
              result = 0;
            }

            string machineAdressFirst = await AppConfiguration.Protocol.ProtocolConfig.GetDeviceInfo() ? $"[{_basePoint.ToString()}]" : string.Empty;
            string machineAdressSecond = await AppConfiguration.Protocol.ProtocolConfig.GetDeviceInfo() ? $"[{point.ToString()}]" : string.Empty;

            var succes = result >= LowerBound && result <= UpperBound;
            var error = new ShowMessageModel(
              $"{_basePoint.Mnemonic}{machineAdressFirst},{point.Mnemonic}{machineAdressSecond} ({LowerBound} - {UpperBound} Ом)",
              message: $"Rизм = {result:F5} Ом",
              type: succes ? ShowMessageModel.MessageType.Success : ShowMessageModel.MessageType.Error)
            { IndentLevel = 3 };

            await messageService.ShowMessageAsync(error);

            if (!succes)
            {
              errorsMessgae.Add(error);
              manager.AddErrorMethod(EhtErrors.ResistanceOutOfRange($"{baseCommandModel.CommandNumber} {baseCommandModel.Mnemonic}", result, _basePoint.ToString(), point.ToString(), LowerBound, UpperBound));
            }
          }

          await DisconnectAllPoints(messageService, chains);
        }
      }

      return errorsMessgae;
    }

    static private async Task ConnectToBusAAndBAsync(IUserMessageService userMessageService, PointModel pointModel)
    {
      await userMessageService.ShowMessageAsync(new ShowMessageModel(header: $"Подключение точки {pointModel.ToString()} к шинам А и В"), IsBlockStart: true);
      var relayModule = EquipmentService.GetModuleByPoint(pointModel);

      if (!await UserActionHelper.GetRunWithUserRepeatAsync(() => relayModule.PointManager.ConnectRelayAsync(DeviceEnums.BusPoint.A, pointModel.PointNumber, userMessageService), userMessageService))
        throw RelayExceptionFactory.ConnectPointFailed(pointModel.PointNumber.ToString(), relayModule.Name, relayModule.NumberChassis, relayModule.Number);

      if (!await UserActionHelper.GetRunWithUserRepeatAsync(() => relayModule.PointManager.ConnectRelayAsync(DeviceEnums.BusPoint.B, pointModel.PointNumber, userMessageService), userMessageService))
        throw RelayExceptionFactory.ConnectPointFailed(pointModel.PointNumber.ToString(), relayModule.Name, relayModule.NumberChassis, relayModule.Number);
    }

    static private async Task DisconnectToBusBAsync(IUserMessageService userMessageService, PointModel pointModel)
    {
      await userMessageService.ShowMessageAsync(new ShowMessageModel(header: $"Отключение точки {pointModel.ToString()} от шины В"), IsBlockStart: true);
      var relayModule = EquipmentService.GetModuleByPoint(pointModel);

      if (!await UserActionHelper.GetRunWithUserRepeatAsync(() => relayModule.PointManager.DisconnectRelayAsync(DeviceEnums.BusPoint.B, pointModel.PointNumber, userMessageService), userMessageService))
        throw RelayExceptionFactory.DisconnectPointFailed(pointModel.PointNumber.ToString(), relayModule.Name, relayModule.NumberChassis, relayModule.Number);
    }

    static private async Task DisconnectToBusAAsync(IUserMessageService userMessageService, PointModel pointModel)
    {
      await userMessageService.ShowMessageAsync(new ShowMessageModel(header: $"Отключение точки {pointModel.ToString()} от шины A"), IsBlockStart: true);
      var relayModule = EquipmentService.GetModuleByPoint(pointModel);

      if (!await UserActionHelper.GetRunWithUserRepeatAsync(() => relayModule.PointManager.DisconnectRelayAsync(DeviceEnums.BusPoint.A, pointModel.PointNumber, userMessageService), userMessageService))
        throw RelayExceptionFactory.DisconnectPointFailed(pointModel.PointNumber.ToString(), relayModule.Name, relayModule.NumberChassis, relayModule.Number);
    }

    static private async Task<double> GetResistanceAsync(IUserMessageService userMessageService, double param)
    {
      var fastMeter = EquipmentService.GetFastMeterOrThrow(userMessageService);
      await userMessageService.ShowMessageAsync(new ShowMessageModel(header: $"Измерение сопротивления"), IsBlockStart: true);
      var result = !await AppConfiguration.Execution.ExecutionConfig.GetIsIdleModeEnabled() ? await fastMeter.ResistanceManager.MeasureResistanceAsync() : !await AppConfiguration.Execution.ExecutionConfig.GetIsErrorSimulationEnabled() ? param / 2 : new Random().Next((int)param - 100, (int)param + 100);
      return result;
    }

    static private async Task DisconnectAllPoints(IUserMessageService userMessageService, List<PointModel> points)
    {
      var modules = EquipmentService.GetUniqueModulesByPoints(points);
      foreach (var module in modules)
      {
        if (!await UserActionHelper.GetRunWithUserRepeatAsync(() => module.PointManager.DisconnectingAllPoint(userMessageService), userMessageService))
          throw RelayExceptionFactory.DisconnectAllPointFailed();
      }
    }
  }
}
