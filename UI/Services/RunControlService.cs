using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DTO.Base.Models;
using Message;
using UI.Components;
using UI.Components.Invoke;
using UI.Components.MultiEditorMethods;
using UI.Controls.Runner;
using UI.Controls.TextEditor;
using static Utilities.LoggerUtility;

namespace UI.Services
{
  public class RunControlService
  {
    public async Task AddRunItem(RunControl runControl, EditorType editorType)
    {
      try
      {
        TextEditorContainer textEditorContainer = _fileManager.ContainerService.GetContainer(editorType);
        if (textEditorContainer == null)
        {
          textEditorContainer = _fileManager.ContainerService.CreateContainer(editorType, OpenFileButton.TypeWindow.DeviceControl);
        }
        _fileManager.DockItemService.ShowNewDockItem($"{runControl.FileName}", textEditorContainer, runControl, editorType);

        _fileManager.ControlManagerService.ShowControl(textEditorContainer, EditorType.Translator);
      }
      catch (Exception ex)
      {
        MessageBoxCustom.Show($"Ошибка при чтении файла: {ex.Message}", "Ошибка", image: MessageBoxImage.Error);
        LogException($"Ошибка при чтении файла", ex);
        return;
      }
    }

    public async Task CloseRunItem(RunControl runControl, EditorType editorType)
    {
      var controlManager = new ControlManager(_fileManager, _fileManager.EditorWorkspaceModel.MultiEditorControl);
      TextEditorContainer runContainer = _fileManager.ContainerService.GetContainer(editorType);
      var foundTab = _fileManager.EditorWorkspaceModel.OpenPages.FirstOrDefault(tab => tab.Text == editorType.ToString());
      if (foundTab != null)
      {
        await controlManager.RemoveControl(foundTab, runControl);
      }
    }

    public RunControlService(UI.Components.MultiEditorMethods.FileManager fileManager)
    {
      _fileManager = fileManager;
    }

    UI.Components.MultiEditorMethods.FileManager _fileManager;
  }
}
