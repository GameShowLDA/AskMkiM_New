using ControlCommandAnalyser.Model;
using ControlCommandExecutor.Execution;
using ControlCommandExecutor.Executors.Interface;
using DTO.Base.Models;
using DTO.Device.RelaySwitchModule.Model;
using DTO.Enum;
using Errors.Device.Adapters;
using Utilities;

namespace ControlCommandExecutor.Executors
{
  internal class RmCommandExecutor : ICommandExecutor
  {
    public string Mnemonic => Utilities.EnumExtensions.GetDisplayOrganizationalInfo(DTO.Enum.Measurement.OrganizationalComands.RM).DisplayName;

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

      var unique = context.GetUniqueMeasurementDevices();

      if (unique.Contains(MeasurementDevice.Multimeter))
      {
        var meter = EquipmentService.GetFastMeterOrThrow(context.Console);

        if (!await UserActionHelper.GetRunWithUserRepeatAsync(async () => (await meter.ConnectableManager.InitializeAsync(context.Console)).Connect, context.Console))
        {
          throw ConnectionExceptionAdapter.ConnectFailed(meter.Name, meter.NumberChassis, meter.Number);
        }
      }

      if (unique.Contains(MeasurementDevice.BreakdownTester))
      {
        var breakDown = await EquipmentService.GetBreakdownTesterOrThrow(context.Console);

        if (!await UserActionHelper.GetRunWithUserRepeatAsync(async () => (await breakDown.ConnectableManager.InitializeAsync(context.Console)).Connect, context.Console))
        {
          throw ConnectionExceptionAdapter.ConnectFailed(breakDown.Name, breakDown.NumberChassis, breakDown.Number);
        }
      }
    }
  }
}
