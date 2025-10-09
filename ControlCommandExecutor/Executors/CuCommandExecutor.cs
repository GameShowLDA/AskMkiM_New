using System.Windows;
using ControlCommandAnalyser;
using ControlCommandAnalyser.Model;
using ControlCommandExecutor.Execution;
using DTO.Base.Models;
using Message;

namespace ControlCommandExecutor.Executors
{
  public class CuCommandExecutor : ICommandExecutor
  {
    public string Mnemonic => "ЦУ";

    public async Task ExecuteAsync(CommandExecutionContext context, ProtocolModel protocolModel)
    {
      var cu = (CuCommandModel)context.Command;
      if (cu.CuType == CuCommandType.Information)
      {
        MessageBoxCustom.Show(cu.MessageText, "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        CommandExecutionState.LastCuResult = MessageBoxResult.OK;
      }
      else if (cu.CuType == CuCommandType.Question)
      {
        // Вопрос — вызываем с кнопками Yes/No/Esc (или Ok/Cancel если Run/Esc)
        var result = MessageBoxCustom.Show(
            cu.MessageText,
            "Вопрос",
            MessageBoxButton.YesNo, MessageBoxImage.Question
        );
        CommandExecutionState.LastCuResult = result;
      }
    }
  }
}
