using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DTO.Base.Models;
using Message;
using UI.Components.MultiEditorMethods;
using UI.Controls;
using UI.Controls.TextEditor;
using static DTO.Enum.FileEnums;
using static Utilities.LoggerUtility;

namespace UI.Services
{
  public class TranslationService
  {
    /// <summary>
    /// Создает текстовый редактор с результатами трансляции файла.
    /// </summary>
    /// <returns>Текстовый редактор с странслированным файлом.</returns>
    public TextEditorUI CreateTranslationFileAsync()
    {
      string fileName = $"Трансляция_{DateTime.Now:HHmmss}.opkw";
      var textEditorModel = new TextEditorModel(fileName);

      var textEditor = new TextEditorUI(FileType.OPKW, textEditorModel)
      {
        Text = "// Результат трансляции появится здесь...",
        IsReadOnly = true
      };

      return textEditor;
    }

    /// <summary>
    /// Выполняет добавление <see cref="TranslatorItem"/> в качестве новой вкладки в DockControl.
    /// </summary>
    /// <param name="editor">Текстовый редактор с транслируемым файлом.</param>
    /// <param name="translateEditor">Текстовый редактор с странслированным файлом.</param>
    /// <param name="editorType">Тип контейнера.</param>
    /// <returns>Асинхронную задачу, представляющую результат выполнения.</returns>
    public async Task<TranslatorItem> AddTranslatorItem(TextEditorUI editor, TextEditorUI translateEditor, EditorType editorType)
    {
      try
      {
        TextEditorContainer textEditorContainer = _fileManager.ContainerService.GetContainer(editorType);
        if (textEditorContainer == null)
        {
          textEditorContainer = _fileManager.ContainerService.CreateContainer(editorType);
        }
        var item = await _fileManager.DockItemService.ShowNewDockItem($"Трансляция {editor.TextEditorModel.FileName}", textEditorContainer, editor, translateEditor);

        _fileManager.ControlManagerService.ShowControl(textEditorContainer, EditorType.Translator);
        return item;
      }
      catch (Exception ex)
      {
        MessageBoxCustom.Show($"Ошибка при чтении файла: {ex.Message}", "Ошибка", image: MessageBoxImage.Error);
        LogException($"Ошибка при чтении файла", ex);
        return null;
      }
    }

    public async Task DeleteTranslatorItem(TranslatorItem translatorItem, EditorType editorType)
    {
      try
      {
        TextEditorContainer textEditorContainer = _fileManager.ContainerService.GetContainer(editorType);
        if (textEditorContainer == null)
        {
          return;
        }

        textEditorContainer.RemoveTranslatorItem(translatorItem);
        if (textEditorContainer.DockManager.DockItems.Count == 0)
        {
          _fileManager.ContainerService.RemoveTextEditorContainer(textEditorContainer, EditorType.Translator);
        }
      }
      catch (Exception ex)
      {
        MessageBoxCustom.Show($"Ошибка при чтении файла: {ex.Message}", "Ошибка", image: MessageBoxImage.Error);
        LogException($"Ошибка при чтении файла", ex);
        return;
      }
    }

    public TranslationService(UI.Components.MultiEditorMethods.FileManager fileManager)
    {
      _fileManager = fileManager;
    }

    UI.Components.MultiEditorMethods.FileManager _fileManager;
  }
}
