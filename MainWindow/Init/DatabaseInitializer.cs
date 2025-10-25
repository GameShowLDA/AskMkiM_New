using AppConfiguration.Execution;
using AppConfiguration.Protocol;
using DataBaseConfiguration;
using DTO.Base.Models;
using static Utilities.LoggerUtility;


namespace MainWindowProgram.Init
{
  static internal class DatabaseInitializer
  {
    static internal async Task InitializeAsync()
    {
      try
      {
        await DataBaseConfig.InitializeDB();

        var protocolTask = new DataBaseConfiguration.Services.Settings.ProtocolService().GetProtocolAsync();
        var executionTask = new DataBaseConfiguration.Services.Settings.ExecutionService().GetExecutionAsync();

        Task.WaitAll(protocolTask, executionTask);

        var protocol = protocolTask.Result;
        var execution = executionTask.Result;

        if (protocol != null)
        {
          await ProtocolConfig.SetProtocolModel(protocol);
          ProtocolModel.SetTemplate(protocol.CleanTextProtocol);
          ProtocolModel.SetErrorsTemplate(protocol.CleanTextErrorsProtocol);
        }

        if (execution != null)
        {
          await ExecutionConfig.SetExecutionModel(execution);
        }

        ProtocolConfig.SaveProtocolEvent += async (model) =>
        {
          var service = new DataBaseConfiguration.Services.Settings.ProtocolService();
          await service.SaveProtocolAsync(model);
          ProtocolModel.SetTemplate(model.CleanTextProtocol);
          ProtocolModel.SetErrorsTemplate(model.CleanTextErrorsProtocol);
        };

        ExecutionConfig.SaveExecutionEvent += async (model) =>
        {
          var service = new DataBaseConfiguration.Services.Settings.ExecutionService();
          await service.SaveExecutionAsync(model);
        };
      }
      catch (Exception ex)
      {
        LogException(ex);
      }

    }
  }
}
