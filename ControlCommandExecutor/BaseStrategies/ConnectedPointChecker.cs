using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ControlCommandAnalyser.Model;
using ControlCommandAnalyser.Model.Chains;
using ControlCommandExecutor.Execution;
using Utilities;
using Utilities.Interface;
using Utilities.Models;
using static ControlCommandExecutor.BaseStrategies.NodeAccumulationChecker;

namespace ControlCommandExecutor.BaseStrategies
{
  /// <summary>
  /// Класс <see cref="ConnectedPointChecker"/> предназначен для проверки электрической
  /// последовательности соединённых точек в схеме и выявления разрывов цепей.
  /// </summary>
  internal static class ConnectedPointChecker
  {
    /// <summary>
    /// Делегат, определяющий метод выполнения измерений.
    /// </summary>
    /// <param name="value">Заданное значение сопротивления для измерения.</param>
    /// <param name="userMessageService">Сервис для отображения сообщений пользователю.</param>
    /// <param name="cancellationToken">Токен отмены для управления асинхронной операцией.</param>
    /// <returns>
    /// Асинхронная операция, возвращающая <c>true</c>, если измерение прошло успешно,
    /// или <c>false</c>, если обнаружена ошибка.
    /// </returns>
    internal delegate Task<(bool Result, string Value)> PerformMeasurementAsync(double value, IUserMessageService userMessageService, CancellationToken cancellationToken);

    /// <summary>
    /// Асинхронно выполняет проверку соединённых точек в схеме.
    /// </summary>
    /// <param name="schemeModel">Модель схемы, содержащая список соединённых точек.</param>
    /// <param name="performMeasurementAsync">
    /// Делегат для выполнения измерения сопротивления между базовой и проверяемой точками.
    /// </param>
    /// <param name="manager">
    /// Экземпляр <see cref="CommandExecutionManager"/>, используемый для регистрации ошибок выполнения.
    /// </param>
    /// <param name="baseCommandModel">
    /// Модель базовой команды, содержащая данные для формирования ошибок.
    /// </param>
    /// <param name="messageService">Сервис для отображения сообщений пользователю.</param>
    /// <param name="resistance">Заданное значение сопротивления для проверки.</param>
    /// <returns>
    /// Асинхронная операция, возвращающая список сообщений об ошибках
    /// (<see cref="ShowMessageModel"/>), если были обнаружены разрывы цепей;
    /// в противном случае возвращается пустой список.
    /// </returns>
    static public async Task<List<ShowMessageModel>> CheckSequenceAsync(SchemeModel schemeModel, PerformMeasurementAsync performMeasurementAsync, CommandExecutionManager manager, BaseCommandModel baseCommandModel, IUserMessageService messageService, double resistance)
    {
      List<ShowMessageModel> errorsMessage = new List<ShowMessageModel>();
      Dictionary<List<PointModel>, string> errorChain = new();
      var pointsList = schemeModel.GetPointsConnected();
      if (pointsList.Count == 0)
      {
        return errorsMessage;
      }

      await messageService.ShowMessageAsync(new ShowMessageModel($"Проверка сообщенных точек"));

      for (int i = 0; i < pointsList.Count; i++)
      {
        var chains = pointsList[i];

        for (int j = 0; j < chains.Count; j++)
        {
          var points = chains[j];
          string chainsStr = "*";
          for (int z = 0; z < points.Count; z++)
          {
            if ((z + 1) == points.Count)
            {
              chainsStr += $"{points[z].Mnemonic}({points[z].ToString()})*";
            }
            else
            {
              chainsStr += $"{points[z].Mnemonic}({points[z].ToString()}),";
            }
          }

          await messageService.ShowMessageAsync(new ShowMessageModel($"Проверка цепи", message: chainsStr, type: ShowMessageModel.MessageType.CommandBlock) { IndentLevel = 1 }, IsBlockStart: true);
          var _basePoint = points[0];
          points.Remove(_basePoint);
          await messageService.ShowMessageAsync(new ShowMessageModel($"Подлючение точек") { IndentLevel = 1 }, IsBlockStart: true);
          await ConnectToBusBAsync(_basePoint, messageService);


          foreach (var point in points)
          {
            messageService.GetCancellationToken().ThrowIfCancellationRequested();
            await messageService.ShowMessageAsync(new ShowMessageModel($"Проверка {point.Mnemonic}({point.ToString()})") { IndentLevel = 1 }, IsBlockStart: true);
            await ConnectToBusAAsync(point, messageService);

            var result = await performMeasurementAsync(resistance, messageService, messageService.GetCancellationToken());
            if (!result.Result)
            {
              errorChain.Add(new List<PointModel>() { _basePoint, point }, result.Value);
            }

            await DisconnectFromBusAAsync(point, messageService);
          }
          await DisconnectFromBusBAsync(_basePoint, messageService);
        }
      }

      if (errorChain.Count > 0)
      {
        await messageService.ShowMessageAsync(
          new ShowMessageModel($"Результаты проверки")
          { IndentLevel = 1 });


        foreach (var item in errorChain.Keys)
        {
          var chain = new ChainModel(item);
          var chainStr = PointFormater.GetFormatConnectPoint(chain);

          var error = new ShowMessageModel($"{chainStr}", message: $"Обнаружен разрыв цепи с результатом - {errorChain.GetValueOrDefault(item)}", type: ShowMessageModel.MessageType.Error) { IndentLevel = 3 };

          await messageService.ShowMessageAsync(error);
          manager.AddErrorMethod(baseCommandModel.PointErrors.DisconnectChainError($"{baseCommandModel.CommandNumber} {baseCommandModel.Mnemonic}", chainStr));
          errorsMessage.Add(error);
        }
      }

      return errorsMessage;
    }

    /// <summary>
    /// Подключает указанную точку к шине B через соответствующий модуль коммутации.
    /// В случае неудачи предлагает пользователю повторить попытку.
    /// </summary>
    /// <param name="point">Точка, которую необходимо подключить к шине B.</param>
    /// <param name="messageService">Сервис для отображения сообщений и взаимодействия с пользователем.</param>
    /// <exception cref="RelayControlException">
    /// Выбрасывается при невозможности подключения точки после всех попыток.
    /// </exception>
    private static async Task ConnectToBusBAsync(PointModel point, IUserMessageService messageService)
    {
      var module = EquipmentService.GetModuleByPoint(point);
      if (!await UserActionHelper.GetRunWithUserRepeatAsync(() => module.PointManager.ConnectRelayAsync(bus: NewCore.Enum.DeviceEnum.BusPoint.B, point.PointNumber, messageService), messageService))
      {
        throw AppConfiguration.Error.Device.ModuleRelayControl.RelayExceptionFactory.ConnectPointFailed(point.PointNumber.ToString(), module.Name, module.NumberChassis, module.Number);
      }
    }

    /// <summary>
    /// Подключает указанную точку к шине A через соответствующий модуль коммутации.
    /// В случае неудачи предлагает пользователю повторить попытку.
    /// </summary>
    /// <param name="point">Точка, которую необходимо подключить к шине A.</param>
    /// <param name="messageService">Сервис для отображения сообщений и взаимодействия с пользователем.</param>
    /// <exception cref="RelayControlException">
    /// Выбрасывается при невозможности подключения точки после всех попыток.
    /// </exception>
    private static async Task ConnectToBusAAsync(PointModel point, IUserMessageService messageService)
    {
      var module = EquipmentService.GetModuleByPoint(point);
      if (!await UserActionHelper.GetRunWithUserRepeatAsync(() => module.PointManager.ConnectRelayAsync(bus: NewCore.Enum.DeviceEnum.BusPoint.A, point.PointNumber, messageService), messageService))
      {
        throw AppConfiguration.Error.Device.ModuleRelayControl.RelayExceptionFactory.ConnectPointFailed(point.PointNumber.ToString(), module.Name, module.NumberChassis, module.Number);
      }
    }


    /// <summary>
    /// Отключает указанную точку от шины A через соответствующий модуль коммутации.
    /// В случае неудачи предлагает пользователю повторить попытку.
    /// </summary>
    /// <param name="point">Точка, которую необходимо отключить от шины A.</param>
    /// <param name="messageService">Сервис для отображения сообщений и взаимодействия с пользователем.</param>
    /// <exception cref="RelayControlException">
    /// Выбрасывается при невозможности отключить точку после всех попыток.
    /// </exception>
    private static async Task DisconnectFromBusAAsync(PointModel point, IUserMessageService messageService)
    {
      var module = EquipmentService.GetModuleByPoint(point);
      if (!await UserActionHelper.GetRunWithUserRepeatAsync(() => module.PointManager.DisconnectRelayAsync(bus: NewCore.Enum.DeviceEnum.BusPoint.A, point.PointNumber, messageService), messageService))
      {
        throw AppConfiguration.Error.Device.ModuleRelayControl.RelayExceptionFactory.DisconnectPointFailed(point.PointNumber.ToString(), module.Name, module.NumberChassis, module.Number);
      }
    }

    /// <summary>
    /// Отключает указанную точку от шины A через соответствующий модуль коммутации.
    /// В случае неудачи предлагает пользователю повторить попытку.
    /// </summary>
    /// <param name="point">Точка, которую необходимо отключить от шины A.</param>
    /// <param name="messageService">Сервис для отображения сообщений и взаимодействия с пользователем.</param>
    /// <exception cref="RelayControlException">
    /// Выбрасывается при невозможности отключить точку после всех попыток.
    /// </exception>
    private static async Task DisconnectFromBusBAsync(PointModel point, IUserMessageService messageService)
    {
      var module = EquipmentService.GetModuleByPoint(point);
      if (!await UserActionHelper.GetRunWithUserRepeatAsync(() => module.PointManager.DisconnectRelayAsync(bus: NewCore.Enum.DeviceEnum.BusPoint.B, point.PointNumber, messageService), messageService))
      {
        throw AppConfiguration.Error.Device.ModuleRelayControl.RelayExceptionFactory.DisconnectPointFailed(point.PointNumber.ToString(), module.Name, module.NumberChassis, module.Number);
      }
    }
  }
}
