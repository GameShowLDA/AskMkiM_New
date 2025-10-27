using AppConfiguration.Execution;
using AppConfiguration.Parameter;
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
        var userInterfaceTask = new DataBaseConfiguration.Services.Settings.UserInterfaceService().GetUserInterfaceAsync();

        Task.WaitAll(protocolTask, executionTask, userInterfaceTask);

        var protocol = protocolTask.Result;
        var execution = executionTask.Result;
        var userInreface = userInterfaceTask.Result;

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

        if (userInreface != null)
        {
          await UserInterfaceConfig.SetUserInterfaceModel(userInreface);
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

        UserInterfaceConfig.SaveUserInterfaceEvent += async (model) =>
        {
          var service = new DataBaseConfiguration.Services.Settings.UserInterfaceService();
          await service.SaveUserInterfaceAsync(model);
        };
      }
      catch (Exception ex)
      {
        LogException(ex);
      }

    }
  }
}
