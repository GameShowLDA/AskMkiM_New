using ControlCommandAnalyser.Model;
using ControlCommandExecutor.Execution;
using ControlCommandExecutor.Executors.Interface;
using DTO.Base.Models;

namespace ControlCommandExecutor.Executors
{
  internal class CpCommandExecutor : ICommandExecutor
  {
    public string Mnemonic => Utilities.EnumExtensions.GetDisplayOrganizationalInfo(DTO.Enum.Measurement.OrganizationalComands.CP).DisplayName;

    public Task ExecuteAsync(CommandExecutionContext context, ProtocolModel protocolModel)
    {
      var command = context.Command as CpCommandModel;
      context.TranslationControl.SetActiveLine(command.FormattedStartLineNumber);

      return Task.CompletedTask;
    }
  }
}
