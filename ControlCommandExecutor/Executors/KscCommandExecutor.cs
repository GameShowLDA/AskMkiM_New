using AppConfiguration.Base;
using ControlCommandAnalyser.Model;
using ControlCommandExecutor.Execution;
using Utilities.ResultProtocol;

namespace ControlCommandExecutor.Executors
{
  internal class KscCommandExecutor : ICommandExecutor
  {
    public string Mnemonic => "КЦ";

    public async Task ExecuteAsync(CommandExecutionContext context, ProtocolModel protocolModel)
    {
      EventAggregator.ProtocolInfoClose -= OnProtocolInfoClosing;
      EventAggregator.ProtocolInfoClose += OnProtocolInfoClosing;
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
        EventAggregator.RaiseGetProtocolInfo(protocolModel);
      }
    }

    private async void OnProtocolInfoClosing(string number, string executor, string agent, string customer, ProtocolModel protocolModel)
    {
      protocolModel.Number = number;
      protocolModel.Executor = executor;
      protocolModel.Agent = agent;
      protocolModel.Customer = customer;
      protocolModel.Mode = await AppConfiguration.Execution.ExecutionConfig.GetIsIdleModeEnabled() ? "Холостой режим" : "Рабочий режим";
      ProtocolModel.GetPathProtocol(protocolModel);
      EventAggregator.RaiseViewProtocol(protocolModel);
      EventAggregator.ProtocolInfoClose -= OnProtocolInfoClosing;
    }
  }
}
