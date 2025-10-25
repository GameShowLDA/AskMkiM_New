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
    public void SubscribeToMessageEvents()
    {
      EventAggregator.Subscribe<Error>(e =>
         messageHandler.SetErrorMessage(e.Text, e.ClearPrevious));

      EventAggregator.Subscribe<Warning>(e =>
        messageHandler.SetWarningMessage(e.Text, e.ClearPrevious));

      EventAggregator.Subscribe<Info>(e =>
        messageHandler.SetInfoMessage(e.Text, e.ClearPrevious));

      EventAggregator.Subscribe<Clear>(_ =>
        messageHandler.ClearMessage());
    }
  }
}
