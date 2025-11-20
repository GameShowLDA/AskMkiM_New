using ControlCommandAnalyser;
using ControlCommandAnalyser.Model.Ok;
using DTO.Base.Interface;
using DTO.Base.Models;
using EventCore.Adapters;
using ICSharpCode.AvalonEdit;
using Message;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using UI.Components.SearchControls;
using UI.Controls;
using UI.Controls.Runner;
using UI.Controls.TextEditor;
using UI.Services;
using UI.Windows.WpfDocking.Windows.Docking;
using Utilities;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace MainWindowProgram.Services
{
  /// <summary>
  /// Сервис трансляции команд из текстового редактора.
  /// Обеспечивает распознавание команд, отображение результатов трансляции и работу с двумя редакторами: исходным и переводом.
  /// </summary>
  public class TranslationServices
  {
    /// <summary>
    /// Сервис для управления многооконным интерфейсом.
    /// </summary>
    private readonly MultiWindowService _multiWindow;

    /// <summary>
    /// Сервис для работы с файлами.
    /// </summary>
    private readonly FileService _fileService;

    private TextEditorUI _actualTextEditor;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="AdminServices"/>.
    /// </summary>
    /// <param name="multiWindow">Сервис управления многооконным интерфейсом.</param>
    /// <param name="fileService">Сервис  для работы с файлами.</param>
    public TranslationServices(MultiWindowService multiWindow, FileService fileService)
    {
      _multiWindow = multiWindow;
      _fileService = fileService;
    }

    /// <summary>
    /// Запускает процесс трансляции текущего открытого текста из редактора.
    /// Выполняет распознавание команд, логирует результат и применяет подсветку
    /// в соответствии с успешностью распознавания.
    /// </summary>
    /// <returns>Задача, представляющая асинхронную операцию трансляции.</returns>
    public async Task BuildAsync()
    {
      var editor = await _multiWindow.GetActiveTextEditor(EditorType.TextEditor);
      var translationContainer = await _multiWindow.GetActiveTextEditorContainer(EditorType.Translator);

      if (editor == null && translationContainer != null)
      {
        await TryUpdateExistingTranslator(translationContainer);
      }
      else if (editor != null)
      {
        await TryCreateNewTranslator(editor);
      }
      else
      {
        ShowEditorNotFoundError();
      }
    }

    /// <summary>
    /// Запускает процесс трансляции текущего открытого текста из редактора.
    /// Выполняет распознавание команд, логирует результат и применяет подсветку
    /// в соответствии с успешностью распознавания.
    /// </summary>
    /// <returns>Задача, представляющая асинхронную операцию трансляции.</returns>
    public async Task RunAsync()
    {
      var editor = await _multiWindow.GetActiveTextEditor(EditorType.TextEditor);
      var container = await _multiWindow.GetActiveTextEditorContainer(EditorType.Translator);
      var runContainer = await _multiWindow.GetActiveTextEditorContainer(EditorType.Run);
      if (container == null && editor != null)
      {
        await BuildAsync();
        editor = await _multiWindow.GetActiveTextEditor(EditorType.TextEditor);
        container = await _multiWindow.GetActiveTextEditorContainer(EditorType.Translator);
      }

      if (container == null && runContainer == null && editor == null)
      {
        MessageBoxCustom.Show($"Не удалось запустить исполнитель программы контроля.", "Ошибка запуска программы контроля", image: MessageBoxImage.Error);
        return;
      }

      if (container == null && runContainer != null)
      {
        container = runContainer;
      }
      var dockManager = container.GetDockControl();
      if (dockManager == null) return;

      DockItem? foundDockItem = null;

      // Ждём, пока хотя бы один DockItem появится
      for (int i = 0; i < 500; i++) // максимум 500 мс
      {
        if (dockManager.DockItems.Count > 0)
        {
          foundDockItem = dockManager.DockItems.FirstOrDefault(item => item.IsActiveItem == true);
          if (foundDockItem != null)
          {
            break;
          }
        }
        await Task.Delay(10);
      }

      if (foundDockItem?.Content is not TranslatorItem translator)
      {
        if (foundDockItem?.Content is RunControl run)
        {
          //RunControl runControl = new RunControl();
          //runControl.TranslationModels = run.TranslationModels;
          await PrepareRun(runContainer, _actualTextEditor, run);
          // TODO: закрыть вкладку со старым транслятором
          //await _multiWindow.CloseRunItem(run, EditorType.Run);
        }
        else
        {
          return;
        }
      }
      else
      {
        _actualTextEditor = translator.GetRightEditor();
        if (_actualTextEditor == null)
        {
          ShowEditorNotFoundError();
          return;
        }

        if (translator.ErrorCount > 0)
        {
          MessageBoxCustom.Show($"Возникли ошибки сборки ({translator.ErrorCount} ошибок). Устраните ошибки и повторите попытку.", "Ошибка запуска программы контроля", image: MessageBoxImage.Error);
          return;
        }

        await _multiWindow.DeleteTranslatorItem(translator, EditorType.Translator);


        RunControl runControl = new RunControl();
        runControl.TranslationModels = translator.TranslationModels;
        await PrepareRun(runContainer, _actualTextEditor, runControl);
      }
    }

    private async Task PrepareRun(TextEditorContainer runContainer, TextEditorUI editor, RunControl runControl)
    {
      runControl.OpkFilePath = editor.TextEditorModel.FilePath;
      runControl.SetLeftEditor(editor);
      var foundItem = runControl.TranslationModels.FirstOrDefault(item => item.GetType() == typeof(OkCommandModel));
      if (foundItem != null && foundItem is OkCommandModel okCommandModel)
      {
        runControl.FileName = okCommandModel.ObjectCode;
      }
      runControl.HeaderFile = string.IsNullOrEmpty(editor.TextEditorModel.FileName) ?
        Path.GetFileName(editor.TextEditorModel.FilePath) : editor.TextEditorModel.FileName;

      if (runContainer == null)
      {
        await _multiWindow.AddRunItem(runControl, EditorType.Run);
      }
      else
      {
        //await _multiWindow.CloseRunItem(runControl, EditorType.Run);
        //dockItem.ItemClosed
        var dockItem = runContainer.GetDockControl().DockItems.FirstOrDefault(item => item.Content == runControl);
        if (dockItem != null)
        {
          dockItem.PerformClose();
        }
          
        await _multiWindow.AddRunItem(runControl, EditorType.Run);
      }

      await runControl.Start(runControl.TranslationModels);
    }

    /// <summary>
    /// Пытается создать новый транслятор, используя текст из указанного редактора.
    /// </summary>
    /// <param name="editor">Редактор с исходным текстом.</param>
    private async Task TryCreateNewTranslator(TextEditorUI editor)
    {
      string text = editor.Text;

      if (_multiWindow.RemoveActiveTextEditor(true))
      {
        EditorEventAdapter.RaiseTextEditorContainerClosing(true, editor.TextEditorModel.FileName);
        await CreateNewTranslator(editor, text);
      }
    }

    /// <summary>
    /// Пытается обновить существующий транслятор, если активен соответствующий элемент интерфейса.
    /// </summary>
    /// <param name="container">Контейнер, содержащий редактор трансляции.</param>
    private async Task TryUpdateExistingTranslator(TextEditorContainer container)
    {
      var dockManager = container.GetDockControl();
      if (dockManager == null) return;

      var foundDockItem = dockManager.DockItems.FirstOrDefault(item => item.IsActiveItem == true);
      if (foundDockItem?.Content is not TranslatorItem translator) return;

      var editor = translator.GetLeftEditor();
      if (editor == null)
      {
        ShowEditorNotFoundError();
        return;
      }

      EditExistingTranslator(editor, foundDockItem);
    }

    /// <summary>
    /// Выводит сообщение об ошибке, если редактор не найден.
    /// </summary>
    private void ShowEditorNotFoundError()
    {
      MessageBoxCustom.Show("Редактор не найден", "Ошибка", MessageBoxButton.OK, image: MessageBoxImage.Error);
    }

    /// <summary>
    /// Обновляет существующий компонент транслятора на основе текста из заданного редактора.
    /// Выполняет трансляцию команд и выводит результат во второй (правый) редактор.
    /// </summary>
    /// <param name="editor">Редактор с исходным текстом.</param>
    /// <param name="foundDockItem">Док-элемент, содержащий компонент транслятора.</param>
    private void EditExistingTranslator(TextEditorUI editor, DockItem foundDockItem)
    {
      string text = editor.Text;
      var translateEditor = _fileService.CreateTranslationFileAsync();

      var manager = new CommandTranslationManager();
      var models = manager.ParseAllAndDisplay(text, translateEditor);

      if (foundDockItem.Content is TranslatorItem item)
      {
        item.SetRightEditor(translateEditor);
        item.SetRightEditorName(translateEditor.TextEditorModel.FileName);
        item.TranslationModels = models;
      }
    }

    /// <summary>
    /// Создаёт новый компонент транслятора, содержащий редактор исходного текста и редактор результата трансляции.
    /// Выполняет разбор команд и отображает результат трансляции.
    /// </summary>
    /// <param name="editor">Редактор с исходным текстом.</param>
    /// <param name="text">Текст, подлежащий трансляции.</param>
    /// <returns>Асинхронная задача создания компонента транслятора.</returns>
    private async Task CreateNewTranslator(TextEditorUI editor, string text)
    {
      try
      {
        var translateEditor = _fileService.CreateTranslationFileAsync();
        //text = PkPreprocessor.PreprocessText(text);
        editor.TextArea.Document.Text = text;
        editor.TextArea.TextView.LineTransformers.Add(new BracesCommentColorizer());
        CancellationTokenSource redrawToken = null;

        editor.TextChanged += async (_, __) =>
         {
           redrawToken?.Cancel();
           redrawToken = new CancellationTokenSource();
           var token = redrawToken.Token;

           try
           {
             await Task.Delay(80, token); // ждём, пока пользователь закончит ввод
             if (!token.IsCancellationRequested)
             {
               // безопасный вызов из UI-потока
               Application.Current.Dispatcher.Invoke(() =>
               {
                 editor.TextArea.TextView.Redraw();
               });
             }
           }
           catch (TaskCanceledException)
           {
             // просто игнорируем отменённую задержку
           }
         };


        if (translateEditor != null)
        {
          translateEditor.TextEditorModel.FilePath = editor.TextEditorModel.FilePath;
          var manager = new CommandTranslationManager();
          var models = manager.ParseAllAndDisplay(text, translateEditor);
          manager.SetSourseLines(models);

          EditorEventAdapter.RaiseCloseRunItem(editor);

          var item = await _multiWindow.AddTranslatorItem(editor, translateEditor, EditorType.Translator);
          item.TranslationModels = models;
        }
      }
      catch (Exception ex)
      {
        MessageBoxCustom.Show($"Не удалось запустить трансляцию программы контроля.", "Ошибка запуска программы контроля", image: MessageBoxImage.Error);
        LoggerUtility.LogError($"Не удалось запустить трансляцию программы контроля: {ex}.");

        EditorEventAdapter.RaiseTextEditorActivated(editor);
        await _multiWindow.OpenFileInEditor(editor.TextEditorModel.FilePath);
      }
    }
  }
}
