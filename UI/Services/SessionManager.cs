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

namespace UI.Services
{
  public class SessionManager
  {
    public async Task SaveSession()
    {
      var model = new SessionModel();

      for (int i = 0; i < _fileManager.EditorWorkspaceModel.UserControls.Count; i++)
      {
        if (_fileManager.EditorWorkspaceModel.UserControls[i] is TextEditorContainer container)
        {
          foreach (var dockItem in container.DockManager.DockItems)
          {
            if (dockItem.Content is TextEditorUI editor)
            {
              var filePath = editor.TextEditorModel?.FilePath ?? string.Empty;
              var fileName = editor.TextEditorModel?.FileName ?? dockItem.Title ?? "Безымянный";
              var text = editor.Text ?? string.Empty;

              model.Tabs.Add(new EditorTabSession
              {
                FilePath = filePath,
                FileName = fileName,
                TextContent = text,
                IsModified = _fileManager.FileService.Comparison.HasFileChanged(dockItem)
              });
            }
            else if (dockItem.Content is TranslatorItem translator)
            {
              var leftEditor = translator.GetLeftEditor();
              var filePath = leftEditor.TextEditorModel?.FilePath ?? string.Empty;
              var fileName = leftEditor.TextEditorModel?.FileName ?? "Безымянный";
              var text = leftEditor.Text ?? string.Empty;

              model.Tabs.Add(new EditorTabSession
              {
                FilePath = filePath,
                FileName = fileName,
                TextContent = text,
                IsModified = true
              });
            }
          }
        }
      }

      // определяем активную вкладку
      var activeTab = _fileManager.EditorWorkspaceModel.OpenPages
          .Select((tab, index) => new { tab, index })
          .FirstOrDefault(p => p.tab.Background == (Brush)Application.Current.Resources["ActiveBorderSolidColorBrush"]);

      model.ActiveTabIndex = activeTab?.index ?? 0;

      if (model.Tabs.Count > 0)
      {
        await new SessionService().SaveSessionAsync(model);
      }
    }

    public async Task RestoreSessionAsync(SessionModel model)
    {
      if (model.Tabs.Count == 0)
      {
        Message.MessageBoxCustom.Show("Не удалось найти данные по последней сессии", "Ошибка сессии");
        return;
      }

      // Создаём контейнер (если ещё не создан)

      for (int i = 0; i < model.Tabs.Count; i++)
      {
        var tab = model.Tabs[i];

        // Восстанавливаем модель редактора
        var textEditorModel = new TextEditorModel(tab.FilePath, tab.FileName);
        var editor = new TextEditorUI(_fileManager.FileService.Opening.DetermineFileType(tab.FilePath ?? ""), textEditorModel)
        {
          Text = tab.TextContent
        };
        if (!_fileManager.EditorWorkspaceModel.FilePaths.ContainsKey(tab.FileName))
        {
          _fileManager.EditorWorkspaceModel.FilePaths.Add(tab.FileName, tab.FilePath);
        }

        bool protocol = tab.FileName.Contains(".lst");
        var container = !protocol ? (_fileManager.ContainerService.GetContainer(EditorType.TextEditor) ?? _fileManager.ContainerService.CreateContainer(EditorType.TextEditor)) : (_fileManager.ContainerService.GetContainer(EditorType.Protocol) ?? _fileManager.ContainerService.CreateContainer(EditorType.Protocol));

        if (protocol)
        {
          _fileManager.DockItemService.ShowNewDockItem(tab.FileName ?? $"Безымянный_{i + 1}", container, editor, EditorType.Protocol);
          _fileManager.ControlManagerService.ShowControl(container, EditorType.Protocol);
        }
        else
        {
          _fileManager.DockItemService.ShowNewDockItem(tab.FileName ?? $"Безымянный_{i + 1}", container, editor, EditorType.TextEditor);
          _fileManager.ControlManagerService.ShowControl(container, EditorType.TextEditor);
        }

        // Восстанавливаем активную вкладку
        if (model.ActiveTabIndex >= 0 && model.ActiveTabIndex < container.DockManager.DockItems.Count)
        {
          var dockItem = container.DockManager.DockItems[model.ActiveTabIndex];
          dockItem.IsSelected = true;
        }
      }
    }


    public SessionManager(UI.Components.MultiEditorMethods.FileManager fileManager)
    {
      _fileManager = fileManager;
    }

    UI.Components.MultiEditorMethods.FileManager _fileManager;
  }
}
