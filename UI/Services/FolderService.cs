using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO.Base.Models;
using UI.Components.MultiEditorMethods;
using UI.Controls;
using UI.Controls.TextEditor;

namespace UI.Services.Services
{
  public class FolderService
  {
    public void OpenFolder()
    {
      TextEditorContainer textEditorContainer = _fileManager.ContainerService.GetContainer(EditorType.TextEditor);
      if (textEditorContainer == null)
      {
        textEditorContainer = _fileManager.ContainerService.GetContainer(EditorType.Translator);
        if (textEditorContainer == null)
        {
          return;
        }
        else
        {
          var translatorEditor = textEditorContainer.DockManager.DockItems.FirstOrDefault(item => item.IsActiveItem == true);
          if (translatorEditor != null && translatorEditor.Content is TranslatorItem translator)
          {
            var leftEditor = translator.GetLeftEditor();
            OpenFileFolder(leftEditor.TextEditorModel.FilePath);
          }
        }
      }
      else
      {
        var activeTextEditor = textEditorContainer.GetTextEditor();
        if (activeTextEditor != null)
        {
          OpenFileFolder(activeTextEditor.TextEditorModel.FilePath);
        }
      }
    }

    /// <summary>
    /// Открывает папку в проводнике, в которой содержится файл.
    /// </summary>
    /// <param name="path">Путь к файлу.</param>
    public static void OpenFileFolder(string path)
    {
      string folder = Path.GetDirectoryName(path);
      if (!string.IsNullOrEmpty(folder) && Directory.Exists(folder))
      {
        Process.Start(new ProcessStartInfo
        {
          FileName = folder,
          UseShellExecute = true,
          Verb = "open"
        });
      }
    }

    public FolderService(UI.Components.MultiEditorMethods.FileManager fileManager)
    {
      _fileManager = fileManager;
    }

    UI.Components.MultiEditorMethods.FileManager _fileManager;
  }
}
