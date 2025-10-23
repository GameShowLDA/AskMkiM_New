using System.Windows;
using AppConfiguration.Execution;
using AppConfiguration.Parameter;
using AppConfiguration.Protocol;
using DataBaseConfiguration;
using DTO.Base.Models;
using EventCore.Services;
using UI.Theme;
using static EventCore.Events.Message;
using static Utilities.LoggerUtility;

namespace MainWindowProgram.Engine
{
  internal class ApplicationInitializer
  {
    private readonly MessageHandler messageHandler;

    /// <summary>
    /// Конструктор инициализатора приложения.
    /// </summary>
    /// <param name="messageHandler">Обработчик сообщений, используемый для отображения ошибок, предупреждений и информации.</param>
    public ApplicationInitializer(MessageHandler messageHandler)
    {
      this.messageHandler = messageHandler;
    }

    /// <summary>
    /// Запускает полную процедуру инициализации приложения.
    /// </summary>
    public async Task InitializeAsync()
    {
      CheckStatusProgram();
      await StartSettingsAsync();

    }

    /// <summary>
    /// Проверяет, запущен ли уже экземпляр приложения, и предотвращает запуск нескольких экземпляров.
    /// </summary>
    private void CheckStatusProgram()
    {
      bool isNewInstance;
      var mutex = new Mutex(true, "AxionHolding", out isNewInstance);

      if (!isNewInstance)
      {
        MessageBox.Show("Вы не можете запускать несколько экземпляров от Axion Holding. Это реализовано, чтобы избежать перегрузку оборудования АСК-МКИ-М!",
            "ВНИМАНИЕ!", MessageBoxButton.OK, MessageBoxImage.Information);
        LogWarning("Попытка запустить несколько экземпляров.");
        Application.Current.Shutdown();
      }
    }

    /// <summary>
    /// Выполняет асинхронную настройку приложения, загружает настройки темы и регистрирует обработчики событий для сообщений.
    /// </summary>
    private async Task StartSettingsAsync()
    {
      try
      {
        //var executionTask = ExecutionSettingsManager.ReadExecutionModeAsync();
        //var protocolTask = ProtocolSettingsManager.ReadProtocolModeAsync();
        await DataBaseConfig.InitializeDB();

        var protocolTask = new DataBaseConfiguration.Services.Settings.ProtocolService().GetProtocolAsync();
        var executionTask = new DataBaseConfiguration.Services.Settings.ExecutionService().GetExecutionAsync();

      
        if (protocolTask.Result != null)
        {
          await ProtocolConfig.SetProtocolModel(protocolTask.Result);
          ProtocolModel.SetTemplate(protocolTask.Result.CleanTextProtocol);
          ProtocolModel.SetErrorsTemplate(protocolTask.Result.CleanTextErrorsProtocol);
        }

        if (executionTask.Result != null)
        {
          await ExecutionConfig.SetExecutionModel(executionTask.Result);
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

      EventAggregator.Subscribe<Error>(e =>
        messageHandler.SetErrorMessage(e.Text, e.ClearPrevious));

      EventAggregator.Subscribe<Warning>(e =>
        messageHandler.SetWarningMessage(e.Text, e.ClearPrevious));

      EventAggregator.Subscribe<Info>(e =>
        messageHandler.SetInfoMessage(e.Text, e.ClearPrevious));

      EventAggregator.Subscribe<Clear>(_ =>
        messageHandler.ClearMessage());

      LogInformation("Настройки инициализированы.");
    }
  }
}
