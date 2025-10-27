using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using AppConfiguration.Protocol;
using DTO.Base.Models;
using DTO.Service;
using DTO.Service.Models;
using DTO.Settings.SettingsModels;
using Message;
using Utilities;
using static AppConfiguration.Protocol.ProtocolConfig;
using static AppConfiguration.SystemStateManager;
using static DTO.Base.Models.ShowMessageModel;
using static Utilities.DelegateManager;

namespace UI.Controls.ProtocolNew
{
  /// <inheritdoc />
  public partial class ProtocolUI : IUserMessageService
  {
    #region Поля.

    /// <summary>
    /// Последнее отображенное сообщение в протоколе.
    /// </summary>
    ShowMessageModel LastModelMeassage;

    /// <summary>
    /// Возвращает текущий статус пошагового режима.
    /// </summary>
    public bool StepMode => ActionExecutor.StepMode;

    public IButtonService ButtonService { get; set; }

    /// <summary>
    /// Действие, которое будет вызвано при нажатии на кнопку "Повторить".
    /// </summary>
    private Func<Task> _retryAction;


    /// <summary>
    /// Главное окно приложения, используемое для отображения интерфейса.
    /// </summary>
    UIElement _mainWindow;

    /// <summary>
    /// Экземпляр <see cref="ActionExecutor"/>, используемый для выполнения действий.
    /// </summary>
    readonly ActionExecutor ActionExecutor;

    bool _checkPower = true;

    private TaskCompletionSource<IUserMessageService.UserAction> _userActionTcs;

    public ErrorManager Errors;

    #region Делегаты выполнения.

    /// <summary>
    /// Делегат, вызываемый для начала действия.
    /// </summary>
    StartDelegate _startDelegate;

    /// <summary>
    /// Делегат, вызываемый для остановки действия.
    /// </summary>
    StopDelegate _stopDelegate;

    /// <summary>
    /// Делегат, вызываемый для возврата к предыдущему состоянию.
    /// </summary>
    ReturnDelegate _returnDelegate;

    PreActionDelegate _preActionDelegate;

    bool _isRepeatEnabled;
    #endregion

    #endregion

    #region Основные настройки.

    /// <summary>
    /// Устанавливает основные настройки выполнения действий.
    /// </summary>
    /// <param name="MainWindow">Главное окно приложения.</param>
    /// <param name="StartDelegate">Делегат запуска.</param>
    /// <param name="isRepeatEnabled">Флаг разрешения повторного выполнения.</param>
    /// <param name="StopDelegate">Делегат остановки (необязательно).</param>
    /// <param name="ReturnDelegate">Делегат возврата к предыдущему состоянию (необязательно).</param>
    /// <param name="preActionDelegate">Делегат предварительных действий перед запуском (необязательно).</param>
    public void SetSettings(
      UIElement MainWindow,
      StartDelegate StartDelegate,
      bool isRepeatEnabled,
      StopDelegate StopDelegate = null,
      ReturnDelegate ReturnDelegate = null,
      PreActionDelegate preActionDelegate = null,
      bool checkPower = true)
    {
      Errors = new ErrorManager(ErrorListBoxVertical);
      try
      {
        _mainWindow = MainWindow;
        _stopDelegate = StopDelegate;
        _startDelegate = StartDelegate;
        _returnDelegate = ReturnDelegate;
        _preActionDelegate = preActionDelegate;
        _checkPower = checkPower;

        if (ReturnDelegate != null)
        {
          _isRepeatEnabled = true;
        }
      }
      catch (Exception ex)
      {
        LoggerUtility.LogException("Ошибка загрузки элемента", ex);
        throw;
      }
    }

    /// <summary>
    /// Настраивает события для элементов управления.
    /// </summary>
    public void SetEventControls()
    {
      StartMeasureResistanceButtonPreviewMouseDown += async (sender, e) => await StartAsync();
      PauseButtonPreviewMouseDown += async (sender, e) => await PauseAsync();

      TopLayerButtonPreviewMouseDown += StepAround_PreviewMouseDown;
      BottomLayerButtonPreviewMouseDown += StepIn_PreviewMouseDown;

      NextButtonPreviewMouseDown += (sender, e) => Resume();
      ExitButtonPreviewMouseDown += async (sender, e) => await StopAsync();

      LoopMeasureResistanceButtonPreviewMouseDown += (sender, e) => LoopMeasureEvent();
      ReturnMeasureResistanceButtonPreviewMouseDown += (sender, e) => ReturnMeasureEvent();
    }
    #endregion

    #region Основные методы кнопок.

    #region Начало и конец.

    /// <summary>
    /// Прерывает выполнение текущего процесса.
    /// </summary>
    /// <returns>Задача, представляющая асинхронную операцию прерывания выполнения.</returns>
    public async Task AbortExecution() => await ActionExecutor.StopAsync(_stopDelegate, _userActionTcs);

    /// <summary>
    /// Начинает запуск измерения.
    /// </summary>
    /// <returns>Задача, представляющая асинхронную операцию измерения.</returns>
    private async Task StartAsync() => await ActionExecutor.StartAsync(_startDelegate, _stopDelegate, header.Text, _isRepeatEnabled, _preActionDelegate, _checkPower);

    /// <summary>
    /// Завершение текущей выполняемой задачи.
    /// </summary>
    /// <returns>Задача, представляющая асинхронную операцию завершения.</returns>
    private async Task StopAsync() => await ActionExecutor.StopAsync(_stopDelegate, _userActionTcs);

    /// <summary>
    /// Выполняет завершающие действия после завершения процесса.
    /// </summary>
    /// <param name="stopDelegate">Делегат завершения процесса (необязательно).</param>
    /// <returns>Задача, представляющая асинхронную операцию завершения.</returns>
    public async Task FinalizeAsync(StopDelegate stopDelegate = null) => await ActionExecutor.FinalizeAsync(stopDelegate);

    #endregion

    #region Пауза и продолжить.

    /// <summary>
    /// Приостанавливает метод на паузу.
    /// </summary>
    /// <returns></returns>
    public async Task PauseAsync() => await ActionExecutor.PauseAsync(GetCancellationToken(), this);

    /// <summary>
    /// Возобновляет метод после паузы.
    /// </summary>
    public void Resume() => ActionExecutor.Resume(ActionExecutor.StepMode, this, _userActionTcs);

    #endregion

    #region Повтор и зацикливание.

    /// <summary>
    /// Запускает цикл выполнения делегата измерения, отображая кнопки "Остановить" и "Завершить".
    /// </summary>
    private async void LoopMeasureEvent() => await ActionExecutor.LoopMeasureEvent(_returnDelegate, _stopDelegate);

    /// <summary>
    /// Выполняет делегат измерения один раз. Если делегат null, выполняется завершение.
    /// </summary>
    private async void ReturnMeasureEvent() => await ActionExecutor.ReturnMeasureEvent(this, _userActionTcs);

    #endregion

    #region По шагам.

    /// <summary>
    /// Обработчик события нажатия на кнопку "Поверх".
    /// </summary>
    private void StepAround_PreviewMouseDown(object sender, MouseButtonEventArgs e) => ActionExecutor.StepAround_PreviewMouseDown(sender, e);

    /// <summary>
    /// Обработчик события нажатия на кнопку "Вглубь".
    /// </summary>
    private void StepIn_PreviewMouseDown(object sender, MouseButtonEventArgs e) => ActionExecutor.StepIn_PreviewMouseDown(sender, e);

    #endregion

    #endregion

    #region Методы.

    /// <summary>
    /// Выводит информацию в протокол.
    /// </summary>
    /// <param name="showMessageModel">Модель сообщения.</param>
    /// <returns>Возвращает режим по шагам.</returns>
    public async Task ShowMessageAsync(ShowMessageModel showMessageModel, bool IsBlockStart = false, bool SkipStepModeCheck = false, bool skipPause = false)
    {
      await CheckBlockStart(IsBlockStart);

      if (await GetTimeStart() && showMessageModel.Status != MessageType.Info && showMessageModel.Status != MessageType.Command)
      {
        showMessageModel.Time = _stopwatch.Elapsed.ToString(@"mm\:ss\.fff", CultureInfo.InvariantCulture);
      }

      await ShouldShowDetailedProtocol(showMessageModel);
      await CheckStatus(showMessageModel);

      await protocolTextBox.AppendLineAsync(showMessageModel);

      if (ActionExecutor.IsPaused)
      {
        await ActionExecutor.WaitWhilePausedAsync(GetCancellationToken(), this);
      }

      if (!skipPause)
      {
        await CheckPause(showMessageModel.Status);
      }

      if (StepControlManager.StepMode && !SkipStepModeCheck)
      {
        if (!StepControlManager.IsStepInto && StepControlManager.InsideBlock)
        {
          // Поверх — внутри блока, пропускаем ожидание
        }
        else
        {
          await KeyboardManager.WaitForNextStepKeyAsync(GetCancellationToken());
        }
      }

      await Task.Delay(1);
    }

    /// <summary>
    /// Асинхронно добавляет пустую строку в протокол с заданным уровнем отступа.
    /// </summary>
    /// <param name="indentLevel">Уровень отступа (не используется в текущей реализации).</param>
    public async Task AppendEmptyLineAsync(int indentLevel = 0)
    {
      await protocolTextBox.AppendEmptyLineAsync();
    }

    public int GetLastLineNumberAsync()
    {
      return protocolTextBox.GetLastLineNumberAsync();
    }

    public async Task MoveToLineAsync(int lineNumber)
    {
      await protocolTextBox.MoveToLineAsync(lineNumber);
    }

    /// <summary>
    /// Проверяет, необходимо ли начать новый блок. Если да — завершает предыдущий и начинает новый.
    /// </summary>
    /// <param name="IsBlockStart">Признак начала нового блока.</param>
    private async Task CheckBlockStart(bool IsBlockStart)
    {
      if (IsBlockStart)
      {
        StepControlManager.ExitBlock();
        StepControlManager.EnterBlock();
      }
    }
    private async void ErrorListBoxVertical_ErrorItemDoubleClicked(ErrorItem error)
    {
      await MoveToLineAsync(error.SourceLineNumber);
    }


    /// <summary>
    /// Проверяет статус сообщения и добавляет текстовую приставку и цвет, если статус не является информационным.
    /// </summary>
    /// <param name="showMessageModel">Модель отображаемого сообщения, передаётся по ссылке.</param>
    private async Task CheckStatus(ShowMessageModel showMessageModel)
    {
      if (showMessageModel.Status != MessageType.Info)
      {
        if (string.IsNullOrEmpty(showMessageModel.Message))
        {
          showMessageModel.Message += showMessageModel.GetQualityPrefix();
        }
        else
        {
          var prefix = showMessageModel.GetQualityPrefix();
          if (!showMessageModel.Message.Contains(prefix))
            showMessageModel.Message += " " + prefix;
        }
        showMessageModel.MessageColor = showMessageModel.GetColorMessage();
      }

      await CheckSyntaxHighlighting(showMessageModel);
    }

    /// <summary>
    /// Если статус сообщения — ошибка и включена остановка при ошибке, выполнение ставится на паузу.
    /// </summary>
    /// <param name="Status">Тип сообщения (ошибка, информация, успех).</param>
    private async Task CheckPause(ShowMessageModel.MessageType? Status)
    {
      if (Status == MessageType.Error && await AppConfiguration.Execution.ExecutionConfig.GetIsStopOnErrorEnabled())
      {
        await PauseAsync();
      }
    }

    /// <summary>
    /// Проверяет, нужно ли отображать детализированный протокол.
    /// Если не нужно, удаляет последнее сообщение, если оно допускает удаление и не содержит ошибки выполнения.
    /// </summary>
    /// <param name="showMessageModel">Модель текущего сообщения, которое потенциально будет сохранено как последнее.</param>
    private async Task ShouldShowDetailedProtocol(ShowMessageModel showMessageModel)
    {
      if (!await GetShowDetailedProtocol())
      {
        if (LastModelMeassage != null && LastModelMeassage.CanBeDeleted && !LastModelMeassage.ExecutionError)
        {
          await protocolTextBox.RemoveLastLinesAsync();
        }

        LastModelMeassage = showMessageModel;
      }
    }

    private async Task CheckSyntaxHighlighting(ShowMessageModel showMessageModel)
    {
      if (!AppConfiguration.Parameter.UserInterfaceConfig.GetSyntaxHighlighting())
      {
        showMessageModel.HeaderColor = (Color)Application.Current.Resources["tests.protocol.message.header.foreground"];
        showMessageModel.MessageColor = (Color)Application.Current.Resources["tests.protocol.message.header.foreground"];
        showMessageModel.TimeColor = (Color)Application.Current.Resources["tests.protocol.message.header.foreground"];
      }
    }

    /// <summary>
    /// Полностью очищает протокол и сбрасывает последнее сообщение.
    /// </summary>
    /// <returns>Возвращает признак успешного завершения операции.</returns>
    public async Task<bool> ClearAllMessagesAsync()
    {
      await protocolTextBox.ClearAsync();
      LastModelMeassage = null;

      if (ActionExecutor.IsPaused)
      {
        await ActionExecutor.WaitWhilePausedAsync(GetCancellationToken(), this);
      }

      Errors.ErrorClear();
      return ActionExecutor.StepMode;
    }

    /// <summary>
    /// Асинхронно удаляет блок, содержащий указанную строку, из RichTextBox.
    /// </summary>
    /// <param name="textToRemove">Строка для поиска и удаления.</param>
    /// <returns>True, если блок был найден и удален; иначе False.</returns>
    public async Task<bool> RemoveLineContainingTextAsync(string textToRemove) => await protocolTextBox.RemoveLineContainingTextAsync(textToRemove);

    /// <summary>
    /// Сохраняет протокол в файл с автоматически сгенерированным именем в фоновом режиме асинхронно.
    /// </summary>
    public async Task SaveProtocolAsync(string name)
    {
      string dateTime = DateTime.Now.ToString("yyyy-MM-dd_HH_mm_ss", CultureInfo.CurrentCulture);
      string filename = $"{name}_{dateTime}.txt";
      string fullPath = Path.Combine($"..\\{FileLocations.DataSaveDirectory}", filename);
      if (!Directory.Exists(Path.GetDirectoryName(fullPath)))
      {
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
      }

      var lines = protocolTextBox.Messages.Select(m =>
          $"{m.Header}: {m.Message}");

      await File.WriteAllLinesAsync(fullPath, lines);
    }

    /// <summary>
    /// Выводит протокол на печать.
    /// </summary>
    public void PrintProtocol(IEnumerable<ShowMessageModel> messages)
    {
      PrintDialog printDialog = new PrintDialog();
      if (printDialog.ShowDialog() != true)
        return;

      FlowDocument document = new FlowDocument
      {
        PagePadding = new Thickness(50),
        ColumnWidth = double.PositiveInfinity
      };

      foreach (var model in messages)
      {
        var paragraph = new Paragraph();

        if (!string.IsNullOrWhiteSpace(model.Header))
        {
          paragraph.Inlines.Add(new Run(model.Header)
          {
            Foreground = new SolidColorBrush(model.HeaderColor ?? Colors.Black),
            FontSize = 18,
            FontWeight = FontWeights.Bold
          });
        }

        if (!string.IsNullOrWhiteSpace(model.Message))
        {
          paragraph.Inlines.Add(new Run(": "));

          paragraph.Inlines.Add(new Run(model.Message)
          {
            Foreground = new SolidColorBrush(model.MessageColor ?? Colors.Black),
            FontSize = 18
          });
        }

        document.Blocks.Add(paragraph);
      }

      IDocumentPaginatorSource source = document;
      printDialog.PrintDocument(source.DocumentPaginator, "Печать протокола...");
    }

    #endregion

    /// <summary>
    /// Возвращает токен отмены для текущего действия, если источник не уничтожен.
    /// </summary>
    /// <returns>Токен отмены <see cref="CancellationToken"/> или <see cref="CancellationToken.None"/>.</returns>
    public CancellationToken GetCancellationToken()
    {
      try
      {
        return ActionExecutor?.CancellationTokenSource?.Token ?? CancellationToken.None;
      }
      catch (ObjectDisposedException)
      {
        return CancellationToken.None;
      }
    }

    public Task<bool> AwaitAdminDecisionAsync(string message)
    {
      MessageBoxCustom.Show("В будущем добавить сюда реализацию выбора", image: MessageBoxImage.Error);
      return Task.FromResult(true);
    }

    /// <summary>
    /// Асинхронно ожидает действие пользователя после возникновения ошибки или остановки.
    /// </summary>
    /// <remarks>
    /// Метод создаёт новый <see cref="TaskCompletionSource{TResult}"/> для ожидания выбора пользователя 
    /// (например, продолжить, пропустить или остановить выполнение).  
    /// Если в конфигурации установлено свойство <c>IsStopOnErrorEnabled</c>,  
    /// интерфейс переходит в режим паузы — скрываются все кнопки и отображаются кнопки управления паузой.  
    /// После выбора действия пользователем результат возвращается как значение перечисления 
    /// <see cref="IUserMessageService.UserAction"/>.
    /// </remarks>
    /// <returns>
    /// Задача, представляющая ожидаемое действие пользователя.  
    /// Если режим остановки на ошибке отключён, возвращается <see cref="IUserMessageService.UserAction.None"/>.
    /// </returns>
    public async Task<IUserMessageService.UserAction> WaitUserActionAsync(bool loop = false)
    {
      _userActionTcs = new TaskCompletionSource<IUserMessageService.UserAction>();

      if (await AppConfiguration.Execution.ExecutionConfig.GetIsStopOnErrorEnabled() || loop)
      {

        SetNonVisibleAllButton();
        ShowButtonsOnPause(true);

        return await _userActionTcs.Task;
      }

      return IUserMessageService.UserAction.None;
    }

    public void AddError(ErrorItem errorItem)
    {
      Errors.AddError(errorItem);
    }

  }
}
