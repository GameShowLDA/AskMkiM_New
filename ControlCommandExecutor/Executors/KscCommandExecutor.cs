using ControlCommandAnalyser.Model;
using ControlCommandExecutor.Execution;
using DTO.Base.Models;
using EventCore.Adapters;
using EventCore.Events;

namespace ControlCommandExecutor.Executors
{
  internal class KscCommandExecutor : ICommandExecutor
  {
    public string Mnemonic => "КЦ";

    public async Task ExecuteAsync(CommandExecutionContext context, ProtocolModel protocolModel)
    {
      EventCore.Services.EventAggregator.Unsubscribe<FileInteractionEvents.ProtocolInfoClose>(e => OnProtocolInfoClosing(e.Number, e.Executor, e.Agent, e.Customer, e.Protocol));
      EventCore.Services.EventAggregator.Subscribe<FileInteractionEvents.ProtocolInfoClose>(e => OnProtocolInfoClosing(e.Number, e.Executor, e.Agent, e.Customer, e.Protocol));

      var command = context.Command as KscCommandModel;
      context.TranslationControl.SetActiveLine(command.FormattedStartLineNumber);

      if (!await AppConfiguration.Execution.ExecutionConfig.GetIsIdleModeEnabled())
      {
        await NewCore.Communication.DeviceCommandSender.ResetAllSystem();
      }

      await GetProtocol(context, command, protocolModel);
    }

    private async Task GetProtocol(CommandExecutionContext context, KscCommandModel command, ProtocolModel protocolModel)
    {
      protocolModel.Designation = command.OkCommandModel.ObjectCode;
      protocolModel.ControlObjectName = command.OkCommandModel.ControlObjectName;
      protocolModel.Date = DateTime.Now.Date;
      protocolModel.EndTime = DateTime.Now;

      var opkPath = context.OpkFilePath;
      protocolModel.ProgramPath = string.IsNullOrWhiteSpace(opkPath)
          ? string.Empty
          : opkPath;
      protocolModel.ProgramName = string.IsNullOrWhiteSpace(opkPath)
          ? "Название программы контроля"
          : Path.GetFileName(opkPath);

      if (await AppConfiguration.Protocol.ProtocolConfig.GetGenerateProtocol())
      {
        FileInteractionEventAdapter.RaiseGetProtocolInfo(protocolModel);
      }
    }

    private async void OnProtocolInfoClosing(string number, string executor, string agent, string customer, ProtocolModel protocolModel)
    {
      protocolModel.Number = number;
      protocolModel.Executor = executor;
      protocolModel.Agent = agent;
      protocolModel.Customer = customer;
      protocolModel.Mode = await AppConfiguration.Execution.ExecutionConfig.GetIsIdleModeEnabled() ? "Холостой режим" : "Рабочий режим";
      // TODO: формирование протокола с ошибкой
      //ProtocolModel.GetPathProtocol(protocolModel); 
      FileInteractionEventAdapter.RaiseViewProtocol(protocolModel);
      EventCore.Services.EventAggregator.Unsubscribe<FileInteractionEvents.ProtocolInfoClose>(e => OnProtocolInfoClosing(e.Number, e.Executor, e.Agent, e.Customer, e.Protocol));
    }
  }
}
