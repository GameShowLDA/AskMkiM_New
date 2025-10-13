using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using DTO.Base.Models;
using UI.Components;
using UI.Components.MultiEditorMethods;
using UI.Controls;
using UI.Controls.TextEditor;
using static DTO.Enum.FileEnums;

namespace UI.Services
{
  public class TextEditorService
  {
    /// <summary>
    /// Закрывает вкладку с активным текстовым редактором.
    /// </summary>
    /// <param name="isTranslation">Переменная, показывающая, выполняется закрытие вкладки при трансляции или нет.</param>
    /// <returns>Возвращает <c>true</c>, если вкладка была закрыта, <c>false</c> в противном случае.</returns>
    public bool RemoveActiveTextEditor(bool isTranslation)
    {
      TextEditorContainer textEditorContainer = _fileManager.ContainerService.GetContainer(EditorType.TextEditor);
      var foundDockItem = textEditorContainer.DockManager.DockItems.FirstOrDefault(item => item.IsActiveItem == true);
      if (foundDockItem != null && foundDockItem.Content is TextEditorUI textEditor)
      {
        var controlManager = new ControlManager(_fileManager.EditorWorkspaceModel);
        var foundPage = _fileManager.EditorWorkspaceModel.OpenPages.FirstOrDefault(page => page.Text == EditorType.TextEditor.ToString());
        controlManager.RemoveControl(foundPage, textEditor, isTranslation);
        _fileManager.EditorWorkspaceModel.FilePaths.Remove(foundDockItem.TabText);

        bool closed = foundDockItem.Close();

        if (textEditorContainer.DockManager.DockItems.Count == 0)
        {
          _fileManager.ContainerService.RemoveTextEditorContainer(textEditorContainer, EditorType.TextEditor);
        }

        return closed;
      }
      return false;
    }

    /// <summary>
    /// Создает новый экземпляр <see cref="TextEditorUI"/> и устанавливает его текст.
    /// </summary>
    /// <param name="fileContent">Содержимое файла, которое будет установлено в редактор.</param>
    /// <returns>Новый экземпляр <see cref="TextEditorUI"/>.</returns>
    public TextEditorUI CreateTextEditor(TextEditorModel textEditorModel, string fileContent, FileType fileType = FileType.None)
    {
      var textEditor = new TextEditorUI(fileType, textEditorModel);
      textEditor.Text = fileContent;
      return textEditor;
    }

    /// <summary>
    /// Получает активный текстовый редактор.
    /// </summary>
    /// <returns></returns>
    public TextEditorUI GetActiveTextEditor(EditorType editorType)
    {
      var activeTab = _fileManager.EditorWorkspaceModel.OpenPages.FirstOrDefault(page => page.Background == (Brush)Application.Current.Resources["ActiveBorderSolidColorBrush"]);
      if (activeTab != null && _fileManager.EditorWorkspaceModel.UserControls[_fileManager.EditorWorkspaceModel.OpenPages.IndexOf(activeTab)] is TextEditorContainer textEditorContainer)
      {
        TextEditorContainer foundContainer = _fileManager.ContainerService.GetContainer(editorType);
        if (foundContainer == null)
        {
          return null;
        }
        else if (editorType == EditorType.TextEditor && string.Equals(activeTab.Text, editorType.ToString()))
        {
          return foundContainer.GetTextEditor();
        }
        else
        {
          return null;
        }
      }
      else
      {
        return null;
      }
    }

    public TextEditorUI GetActiveTextEditor()
    {
      var activeTab = _fileManager.EditorWorkspaceModel.OpenPages.FirstOrDefault(page => page.Background == (Brush)Application.Current.Resources["ActiveBorderSolidColorBrush"]);

      if (activeTab != null)
      {
        int index = _fileManager.EditorWorkspaceModel.OpenPages.IndexOf(activeTab);
        if (_fileManager.EditorWorkspaceModel.UserControls[index] is TextEditorContainer textEditorContainer)
        {
          var foundItem = textEditorContainer.DockManager.DockItems.FirstOrDefault(item => item.IsActiveDocument == true);
          if (foundItem != null)
          {
            if (foundItem.Content is TranslatorItem translatorItem)
            {
              return translatorItem.GetLeftEditor();
            }
            else if (foundItem.Content is TextEditorUI foundTextEditor)
            {
              return foundTextEditor;
            }
            else
            {
              return null;
            }
          }
          else
          {
            return null;
          }
        }
        else
        {
          return null;
        }
      }
      else
      {
        return null;
      }
    }


    public TextEditorService(UI.Components.MultiEditorMethods.FileManager fileManager)
    {
      _fileManager = fileManager;
    }

    UI.Components.MultiEditorMethods.FileManager _fileManager;
  }
}
