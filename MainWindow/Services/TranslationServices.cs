using ControlCommandAnalyser;
using ControlCommandAnalyser.Model.Ok;
using DTO.Base.Interface;
using DTO.Base.Models;
using EventCore.Adapters;
using ICSharpCode.AvalonEdit;
using Message;
using System;
using System.Collections.Generic;      // ★ для IReadOnlyCollection<int>
using System.IO;
using System.Linq;                     // ★ для FirstOrDefault
using System.Threading;
using System.Threading.Tasks;
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

    /// <summary>
    /// Редактор, на основании которого был подготовлен текущий RunControl.
    /// Обычно это правый редактор транслятора (внутренний язык).
    /// </summary>
    private TextEditorUI _actualTextEditor;

    public TranslationServices(MultiWindowService multiWindow, FileService fileService)
    {
      _multiWindow = multiWindow;
      _fileService = fileService;
    }

    /// <summary>
    /// Возвращает список строк с установленными точками останова для указанного редактора.
    /// Если редактор null, возвращает пустой список.
    /// </summary>
    private static IReadOnlyCollection<int> GetBreakpointsFromEditor(TextEditorUI editor)
    {
      return editor?.Breakpoints ?? Array.Empty<int>();
    }

    /// <summary>
    /// Запускает процесс трансляции текущего открытого текста из редактора.
    /// Выполняет распознавание команд, логирует результат и применяет подсветку
    /// в соответствии с успешностью распознавания.
    /// </summary>
    public async Task BuildAsync()
    {
      var editor = await _multiWindow.GetActiveTextEditor(EditorType.TextEditor);
      var translationContainer = await _multiWindow.GetActiveTextEditorContainer(EditorType.Translator);

      if (editor is not null) editor.BreakpointsEnabled = false;

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
    /// Обычный запуск программы контроля без учёта точек останова.
    /// (старое поведение, без изменений логики интерпретатора)
    /// </summary>
    public async Task RunAsync()
    {
      var editor = await _multiWindow.GetActiveTextEditor(EditorType.TextEditor);
      var container = await _multiWindow.GetActiveTextEditorContainer(EditorType.Translator);
      var runContainer = await _multiWindow.GetActiveTextEditorContainer(EditorType.Run);

      // Если транслятор ещё не создан — сначала собираем его
      if (container == null && editor != null)
      {
        await BuildAsync();
        editor = await _multiWindow.GetActiveTextEditor(EditorType.TextEditor);
        container = await _multiWindow.GetActiveTextEditorContainer(EditorType.Translator);
      }

      if (container == null && runContainer == null && editor == null)
      {
        MessageBoxCustom.Show(
          "Не удалось запустить исполнитель программы контроля.",
          "Ошибка запуска программы контроля",
          image: MessageBoxImage.Error);
        return;
      }

      // Если транслятор закрыт, но уже открыт исполнитель — работаем через него
      if (container == null && runContainer != null)
      {
        container = runContainer;
      }

      var dockManager = container.GetDockControl();
      if (dockManager == null) return;

      DockItem? foundDockItem = null;

      // Ждём активный DockItem (максимум 500 мс)
      for (int i = 0; i < 500; i++)
      {
        if (dockManager.DockItems.Count > 0)
        {
          foundDockItem = dockManager.DockItems.FirstOrDefault(item => item.IsActiveItem == true);
          if (foundDockItem != null)
            break;
        }
        await Task.Delay(10);
      }

      // --- Случай 1: уже открыт RunControl ---
      if (foundDockItem?.Content is RunControl run)
      {
        // Берём редактор, к которому привязан исполнитель
        var editorFromRun = run.LeftEditor ?? _actualTextEditor;

        if (editorFromRun == null)
        {
          ShowEditorNotFoundError();
          return;
        }

        // Берём актуальные брейкпоинты из этого редактора
        var breakpoints = GetBreakpointsFromEditor(editorFromRun);

        await PrepareRun(runContainer, editorFromRun, run, breakpoints);
        return;
      }

      // --- Случай 2: открыт TranslatorItem ---
      if (foundDockItem?.Content is TranslatorItem translator)
      {
        _actualTextEditor = translator.GetRightEditor();
        if (_actualTextEditor == null)
        {
          ShowEditorNotFoundError();
          return;
        }

        if (translator.ErrorCount > 0)
        {
          MessageBoxCustom.Show(
            $"Возникли ошибки сборки ({translator.ErrorCount} ошибок). Устраните ошибки и повторите попытку.",
            "Ошибка запуска программы контроля",
            image: MessageBoxImage.Error);
          return;
        }

        // Берём брейкпоинты из правого редактора транслятора (внутренний язык)
        var breakpoints = GetBreakpointsFromEditor(_actualTextEditor);

        // Закрываем вкладку транслятора
        await _multiWindow.DeleteTranslatorItem(translator, EditorType.Translator);

        // Создаём RunControl и прокидываем туда модели
        RunControl runControl = new RunControl
        {
          TranslationModels = translator.TranslationModels
        };

        await PrepareRun(runContainer, _actualTextEditor, runControl, breakpoints);
        return;
      }

      // Ничего подходящего не нашли
      return;
    }

    /// <summary>
    /// Запуск программы контроля с учётом точек останова, выставленных в редакторе.
    /// Точки останова передаются в RunControl через свойство Breakpoints.
    /// </summary>
    //public async Task RunWithBreakpointsAsync()
    //{
    //  var editor = await _multiWindow.GetActiveTextEditor(EditorType.TextEditor);
    //  var container = await _multiWindow.GetActiveTextEditorContainer(EditorType.Translator);
    //  var runContainer = await _multiWindow.GetActiveTextEditorContainer(EditorType.Run);

    //  // Если транслятор ещё не создан, создаём его
    //  if (container == null && editor != null)
    //  {
    //    await BuildAsync();
    //    editor = await _multiWindow.GetActiveTextEditor(EditorType.TextEditor);
    //    container = await _multiWindow.GetActiveTextEditorContainer(EditorType.Translator);
    //  }

    //  if (container == null && runContainer == null && editor == null)
    //  {
    //    MessageBoxCustom.Show(
    //      "Не удалось запустить исполнитель программы контроля.",
    //      "Ошибка запуска программы контроля",
    //      image: MessageBoxImage.Error);
    //    return;
    //  }

    //  if (container == null && runContainer != null)
    //  {
    //    container = runContainer;
    //  }

    //  var dockManager = container.GetDockControl();
    //  if (dockManager == null) return;

    //  DockItem? foundDockItem = null;

    //  // Ждём, пока хотя бы один DockItem появится (максимум 500 мс)
    //  for (int i = 0; i < 500; i++)
    //  {
    //    if (dockManager.DockItems.Count > 0)
    //    {
    //      foundDockItem = dockManager.DockItems.FirstOrDefault(item => item.IsActiveItem == true);
    //      if (foundDockItem != null)
    //        break;
    //    }
    //    await Task.Delay(10);
    //  }

    //  if (foundDockItem?.Content is not TranslatorItem translator)
    //  {
    //    // Уже открыта вкладка Run — просто перезапускаем, но с текущими брейкпоинтами
    //    if (foundDockItem?.Content is RunControl run)
    //    {
    //      if (_actualTextEditor == null)
    //      {
    //        ShowEditorNotFoundError();
    //        return;
    //      }

    //      var breakpoints = GetBreakpointsFromEditor(_actualTextEditor);
    //      await PrepareRun(runContainer, _actualTextEditor, run, breakpoints);
    //    }
    //    else
    //    {
    //      return;
    //    }
    //  }
    //  else
    //  {
    //    // Есть активный TranslatorItem: берём правый редактор (внутренний язык)
    //    _actualTextEditor = translator.GetRightEditor();
    //    if (_actualTextEditor == null)
    //    {
    //      ShowEditorNotFoundError();
    //      return;
    //    }

    //    if (translator.ErrorCount > 0)
    //    {
    //      MessageBoxCustom.Show(
    //        $"Возникли ошибки сборки ({translator.ErrorCount} ошибок). Устраните ошибки и повторите попытку.",
    //        "Ошибка запуска программы контроля",
    //        image: MessageBoxImage.Error);
    //      return;
    //    }

    //    // Берём брейкпоинты из этого редактора
    //    var breakpoints = GetBreakpointsFromEditor(_actualTextEditor);

    //    await _multiWindow.DeleteTranslatorItem(translator, EditorType.Translator);

    //    RunControl runControl = new RunControl
    //    {
    //      TranslationModels = translator.TranslationModels
    //    };

    //    await PrepareRun(runContainer, _actualTextEditor, runControl, breakpoints);
    //  }
    //}

    /// <summary>
    /// Подготовка и запуск RunControl.
    /// Если переданы точки останова, они попадают в RunControl.Breakpoints.
    /// </summary>
    private async Task PrepareRun(
      TextEditorContainer runContainer,
      TextEditorUI editor,
      RunControl runControl,
      IReadOnlyCollection<int>? breakpoints = null)      // ★ добавлен параметр
    {
      if (editor == null)
      {
        ShowEditorNotFoundError();
        return;
      }

      runControl.OpkFilePath = editor.TextEditorModel.FilePath;
      runControl.SetLeftEditor(editor);

      var foundItem = runControl.TranslationModels
        .FirstOrDefault(item => item.GetType() == typeof(OkCommandModel));

      if (foundItem is OkCommandModel okCommandModel)
      {
        runControl.FileName = okCommandModel.ObjectCode;
      }

      runControl.HeaderFile = string.IsNullOrEmpty(editor.TextEditorModel.FileName)
        ? Path.GetFileName(editor.TextEditorModel.FilePath)
        : editor.TextEditorModel.FileName;

      // ★ Передаём точки останова в RunControl (свойство нужно добавить в RunControl)
      if (breakpoints != null && breakpoints.Count > 0)
      {
        runControl.Breakpoints = breakpoints;
      }
      else
      {
        runControl.Breakpoints = Array.Empty<int>();
      }

      if (runContainer == null)
      {
        await _multiWindow.AddRunItem(runControl, EditorType.Run);
      }
      else
      {
        var dock = runContainer.GetDockControl();
        var dockItem = dock?.DockItems.FirstOrDefault(item => item.Content == runControl);
        if (dockItem != null)
        {
          dockItem.PerformClose();
        }

        await _multiWindow.AddRunItem(runControl, EditorType.Run);
      }

      await runControl.Start(runControl.TranslationModels);
    }

    // ======= Остальной код без изменений =======

    private async Task TryCreateNewTranslator(TextEditorUI editor)
    {
      string text = editor.Text;

      if (_multiWindow.RemoveActiveTextEditor(true))
      {
        EditorEventAdapter.RaiseTextEditorContainerClosing(true, editor.TextEditorModel.FileName);
        await CreateNewTranslator(editor, text);
      }
    }

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

    private void ShowEditorNotFoundError()
    {
      MessageBoxCustom.Show("Редактор не найден", "Ошибка", MessageBoxButton.OK, image: MessageBoxImage.Error);
    }

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

    private async Task CreateNewTranslator(TextEditorUI editor, string text)
    {
      try
      {
        var translateEditor = _fileService.CreateTranslationFileAsync();
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
              Application.Current.Dispatcher.Invoke(() =>
              {
                editor.TextArea.TextView.Redraw();
              });
            }
          }
          catch (TaskCanceledException)
          {
            // игнорируем отменённую задержку
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
        MessageBoxCustom.Show(
          "Не удалось запустить трансляцию программы контроля.",
          "Ошибка запуска программы контроля",
          image: MessageBoxImage.Error);

        LoggerUtility.LogError($"Не удалось запустить трансляцию программы контроля: {ex}.");

        EditorEventAdapter.RaiseTextEditorActivated(editor);
        await _multiWindow.OpenFileInEditor(editor.TextEditorModel.FilePath);
      }
    }
  }
}