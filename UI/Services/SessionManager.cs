using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using DataBaseConfiguration.Services;
using DTO.Base.Models;
using DTO.Base.Models.Session;
using UI.Components.MultiEditorMethods;
using UI.Controls;
using UI.Controls.TextEditor;
using UI.Services.FileManager;
using UI.Windows.WpfDocking.Windows.Docking;

namespace UI.Services
{
  /// <summary>
  /// Сервис управления сессиями редактора.  
  /// Отвечает за сохранение и восстановление состояния открытых вкладок при завершении и повторном запуске приложения.
  /// </summary>
  public class SessionManager
  {
    private readonly UI.Components.MultiEditorMethods.FileManager _fileManager;

    /// <summary>
    /// Инициализирует новый экземпляр менеджера сессий.
    /// </summary>
    /// <param name="fileManager">Главный файловый менеджер, предоставляющий доступ к редактору и его состоянию.</param>
    public SessionManager(UI.Components.MultiEditorMethods.FileManager fileManager)
    {
      _fileManager = fileManager;
    }

    /// <summary>
    /// Сохраняет текущую сессию редактора: все открытые вкладки, их содержимое, состояние изменений и активную вкладку.
    /// </summary>
    public async Task SaveSessionAsync()
    {
      var model = new SessionModel
      {
        Tabs = CollectOpenTabs(),
        ActiveTabIndex = GetActiveTabIndex()
      };

      if (model.Tabs.Count > 0)
      {
        await new SessionService().SaveSessionAsync(model);
      }
    }

    /// <summary>
    /// Восстанавливает сессию редактора по переданной модели.  
    /// Открывает все вкладки, восстанавливает их содержимое и выделяет активную вкладку.
    /// </summary>
    /// <param name="model">Модель сохранённой сессии, полученная из <see cref="SessionService"/>.</param>
    public async Task RestoreSessionAsync(SessionModel model)
    {
      if (model.Tabs.Count == 0)
      {
        Message.MessageBoxCustom.Show("Не удалось найти данные по последней сессии", "Ошибка сессии");
        return;
      }

      foreach (var (tab, index) in model.Tabs.Select((t, i) => (t, i)))
      {
        await RestoreTabAsync(tab, index, model.ActiveTabIndex);
      }
    }

    /// <summary>
    /// Собирает информацию о всех открытых вкладках редактора и формирует список для сохранения.
    /// </summary>
    /// <returns>Список открытых вкладок в виде коллекции <see cref="EditorTabSession"/>.</returns>
    private List<EditorTabSession> CollectOpenTabs()
    {
      var tabs = new List<EditorTabSession>();

      foreach (var control in _fileManager.EditorWorkspaceModel.UserControls)
      {
        if (control is not TextEditorContainer container)
          continue;

        foreach (var dockItem in container.DockManager.DockItems)
        {
          if (dockItem.Content is TextEditorUI editor)
          {
            tabs.Add(CreateSessionFromEditor(dockItem, editor));
          }
          else if (dockItem.Content is TranslatorItem translator)
          {
            tabs.Add(CreateSessionFromTranslator(translator));
          }
        }
      }

      return tabs;
    }

    /// <summary>
    /// Создаёт объект <see cref="EditorTabSession"/> на основе обычного текстового редактора.
    /// </summary>
    private EditorTabSession CreateSessionFromEditor(DockItem dockItem, TextEditorUI editor)
    {
      return new EditorTabSession
      {
        FilePath = editor.TextEditorModel?.FilePath ?? string.Empty,
        FileName = editor.TextEditorModel?.FileName ?? dockItem.Title ?? "Безымянный",
        TextContent = editor.Text ?? string.Empty,
        IsModified = _fileManager.FileService.Comparison.HasFileChanged(dockItem)
      };
    }

    /// <summary>
    /// Создаёт объект <see cref="EditorTabSession"/> на основе вкладки-транслятора.
    /// </summary>
    private EditorTabSession CreateSessionFromTranslator(TranslatorItem translator)
    {
      var leftEditor = translator.GetLeftEditor();
      return new EditorTabSession
      {
        FilePath = leftEditor.TextEditorModel?.FilePath ?? string.Empty,
        FileName = leftEditor.TextEditorModel?.FileName ?? "Безымянный",
        TextContent = leftEditor.Text ?? string.Empty,
        IsModified = true
      };
    }

    /// <summary>
    /// Определяет индекс активной вкладки в текущей сессии.
    /// </summary>
    /// <returns>Индекс активной вкладки или 0, если активная вкладка не найдена.</returns>
    private int GetActiveTabIndex()
    {
      var activeTab = _fileManager.EditorWorkspaceModel.OpenPages
          .Select((tab, index) => new { tab, index })
          .FirstOrDefault(p => p.tab.Background == (Brush)Application.Current.Resources["ActiveBorderSolidColorBrush"]);

      return activeTab?.index ?? 0;
    }

    /// <summary>
    /// Восстанавливает одну вкладку редактора из сохранённой сессии.
    /// </summary>
    /// <param name="tab">Данные вкладки.</param>
    /// <param name="index">Индекс вкладки.</param>
    /// <param name="activeIndex">Индекс активной вкладки.</param>
    private async Task RestoreTabAsync(EditorTabSession tab, int index, int activeIndex)
    {
      var textEditorModel = new TextEditorModel(tab.FilePath, tab.FileName);
      var editor = new TextEditorUI(_fileManager.FileService.Opening.DetermineFileType(tab.FilePath ?? ""), textEditorModel)
      {
        Text = tab.TextContent
      };

      RegisterFilePath(tab);

      bool isProtocol = tab.FileName.Contains(".lst");
      var container = GetOrCreateContainer(isProtocol);

      await ShowRestoredTabAsync(tab, index, activeIndex, editor, container, isProtocol);
    }

    /// <summary>
    /// Регистрирует путь к файлу, если он ещё не зарегистрирован.
    /// </summary>
    private void RegisterFilePath(EditorTabSession tab)
    {
      if (!_fileManager.EditorWorkspaceModel.FilePaths.ContainsKey(tab.FileName))
      {
        _fileManager.EditorWorkspaceModel.FilePaths.Add(tab.FileName, tab.FilePath);
      }
    }

    /// <summary>
    /// Получает или создаёт контейнер для указанного типа вкладки.
    /// </summary>
    private TextEditorContainer GetOrCreateContainer(bool isProtocol)
    {
      return isProtocol
          ? _fileManager.ContainerService.GetEditorContainer(EditorType.Protocol) ?? _fileManager.ContainerService.CreateEditorContainer(EditorType.Protocol)
          : _fileManager.ContainerService.GetEditorContainer(EditorType.TextEditor) ?? _fileManager.ContainerService.CreateEditorContainer(EditorType.TextEditor);
    }

    /// <summary>
    /// Отображает восстановленную вкладку и устанавливает активную, если необходимо.
    /// </summary>
    private async Task ShowRestoredTabAsync(EditorTabSession tab, int index, int activeIndex, TextEditorUI editor, TextEditorContainer container, bool isProtocol)
    {
      var editorType = isProtocol ? EditorType.Protocol : EditorType.TextEditor;
      var title = tab.FileName ?? $"Безымянный_{index + 1}";

      _fileManager.DockItemService.ShowEditorDockItem(title, container, editor, editorType);
      _fileManager.ControlManagerService.ShowEditorContainer(container, editorType);

      await Task.Delay(1).ConfigureAwait(true);

      if (activeIndex >= 0 && activeIndex < container.DockManager.DockItems.Count)
      {
        container.DockManager.DockItems[activeIndex].IsSelected = true;
      }
    }
  }
}
