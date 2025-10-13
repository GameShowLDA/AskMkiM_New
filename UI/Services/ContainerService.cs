using DTO.Base.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using UI.Components;
using UI.Components.Invoke;
using UI.Components.MultiEditorMethods;
using UI.Controls.TextEditor;
using static Utilities.LoggerUtility;

namespace UI.Services
{
  public class ContainerService
  {
    /// <summary>
    /// Создает контейнер для вкладок заданного типа.
    /// </summary>
    /// <param name="editorType">Тип вкладок.</param>
    /// <returns>Контейнер для вкладок заданного типа.</returns>
    public TextEditorContainer CreateContainer(EditorType editorType, OpenFileButton.TypeWindow fileType = OpenFileButton.TypeWindow.Files)
    {
      var textEditorContainer = new TextEditorContainer();
      AddFileToControlManager(editorType.ToString(), textEditorContainer, fileType);
      return textEditorContainer;
    }

    /// <summary>
    /// Получает контейнер заданного типа.
    /// </summary>
    /// <param name="editorType">Тип контейнера.</param>
    /// <returns>Найденный контейнер или <c>null</c>, если контнейнер не был найден.</returns>
    public TextEditorContainer GetContainer(EditorType editorType)
    {
      var containerPage = _fileManager.EditorWorkspaceModel.OpenPages.FirstOrDefault(page => page.Text == editorType.DisplayName);
      if (containerPage == null)
      {
        return null;
      }
      else
      {
        var foundElement = _fileManager.EditorWorkspaceModel.UserControls[_fileManager.EditorWorkspaceModel.OpenPages.IndexOf(containerPage)];
        if (foundElement != null && foundElement is TextEditorContainer textEditorContainer)
        {
          return textEditorContainer;
        }
        else
        {
          return null;
        }
      }
    }

    /// <summary>
    /// Удаляет контрол с котейнером для текстовых редакторов.
    /// </summary>
    /// <param name="textEditorContainer">Контейнер с текстовыми редакторами.</param>
    public void RemoveTextEditorContainer(TextEditorContainer textEditorContainer, EditorType editorType)
    {
      var controlManager = new ControlManager(_fileManager.EditorWorkspaceModel);
      var foundPage = _fileManager.EditorWorkspaceModel.OpenPages.FirstOrDefault(page => page.Text == editorType.ToString());
      controlManager.RemoveControl(foundPage, textEditorContainer);
    }

    /// <summary>
    /// Добавляет контрол в мультиэдитор.
    /// </summary>
    /// <param name="nameFile">Имя добавляемого файла.</param>
    /// <param name="container">Экземпляр класса <see cref="UserControl"/>, представляющий собой контейнер.</param>
    public void AddFileToControlManager(string nameFile, UserControl container, OpenFileButton.TypeWindow fileType)
    {
      var controlManager = new ControlManager(_fileManager.EditorWorkspaceModel);
      controlManager.AddControl(nameFile, container, fileType);
    }

    public ContainerService(UI.Components.MultiEditorMethods.FileManager fileManager)
    {
      _fileManager = fileManager;
    }

    UI.Components.MultiEditorMethods.FileManager _fileManager;
  }
}
