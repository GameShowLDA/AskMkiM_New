using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO.Base.Models;
using UI.Components;
using UI.Components.Invoke;
using UI.Components.MultiEditorMethods;
using UI.Controls.TextEditor;
using static Utilities.LoggerUtility;


namespace UI.Services
{
  public class ControlManagerService
  {
    /// <summary>
    /// Отображает контейнер для вкладок заданного типа.
    /// </summary>
    /// <param name="textEditorContainer">Контейнер, содержащий вкладки заданного типа.</param>
    /// <param name="editorType">Тип вкладок контейнера.</param>
    public void ShowControl(TextEditorContainer textEditorContainer, EditorType editorType)
    {
      LogDebug($"Отображение контейнера для типа \"{editorType.ToString()}\"");
      var controlManager = new ControlManager(_context);
      var tabButton = new OpenFileButton();
      tabButton.Header.Text = editorType.ToString();
      controlManager.ShowControl(textEditorContainer, tabButton);
    }

    public ControlManagerService(EditorWorkspaceModel editorWorkspaceModel)
    {
      _context = editorWorkspaceModel;
    }

    EditorWorkspaceModel _context;
  }
}
