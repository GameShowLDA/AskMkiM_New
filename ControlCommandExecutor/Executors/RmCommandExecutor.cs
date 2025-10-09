using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlCommandAnalyser.Model;
using ControlCommandAnalyser.Model.Ok;
using ControlCommandExecutor.Execution;
using DTO.Service.Models;
using Utilities.Models;
using Utilities.ResultProtocol;

namespace ControlCommandExecutor.Executors
{
  internal class RmCommandExecutor : ICommandExecutor
  {
    public string Mnemonic => "РМ";

    public async Task ExecuteAsync(CommandExecutionContext context, ProtocolModel protocolModel)
    {

      var command = context.Command as RmCommandModel;
      context.TranslationControl.SetActiveLine(command.FormattedStartLineNumber);
      string message = string.Empty;

      foreach (var str in command.SourceLines)
      {
        message += "\r\n  " + str;
      }
      await context.Console.ShowMessageAsync(new ShowMessageModel($"\r\nРабочее место", headerColor: ShowMessageModel.SuccessMessage.TitleColor, message: message, type: ShowMessageModel.MessageType.Command) { IndentLevel = 1 }, IsBlockStart: true);

      var points = command.GetAllDestinationPoints();

      List<PointModel> pointsModel = PointModel.ConvertToPointModels(points);
      await EquipmentService.AnalyzePoints(pointsModel, command.PointsMap, context.Console);

    }
  }
}
