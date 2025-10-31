using System.Windows;
using ControlCommandAnalyser;
using ControlCommandAnalyser.Model;
using ControlCommandExecutor.Execution;
using DTO.Base.Models;

namespace ControlCommandExecutor.Executors
{
  public class UpCommandExecutor : ICommandExecutor
  {
    public string Mnemonic => Utilities.EnumExtensions.GetDisplayOrganizationalInfo(DTO.Enum.Measurement.OrganizationalComands.UP).DisplayName;


    public async Task ExecuteAsync(CommandExecutionContext context, ProtocolModel protocolModel)
    {
      var up = (UpCommandModel)context.Command;

      if (CommandExecutionState.LastCuResult == MessageBoxResult.No)
      {
        context.JumpToCommandNumber?.Invoke(up.TargetLabel);
      }

      CommandExecutionState.LastCuResult = MessageBoxResult.None;
    }
  }
}
