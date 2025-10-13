using DTO.Base.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UI.Controls.TextEditor;

namespace UI.Services.FileManager
{
  public class FileCreateService
  {
    private readonly UI.Components.MultiEditorMethods.FileManager _fileManager;

    /// <summary>
    /// Инициализирует новый экземпляр сервиса создания файлов.
    /// </summary>
    /// <param name="fileManager">Экземпляр основного файлового менеджера, через который выполняются операции создания и регистрации файлов.</param>
    public FileCreateService(UI.Components.MultiEditorMethods.FileManager fileManager)
    {
      _fileManager = fileManager;
    }

    /// <summary>
    /// Создаёт новый пустой файл и добавляет его в рабочее пространство редактора.
    /// 
    /// При создании:
    /// <list type="bullet">
    ///   <item>Проверяет, существует ли контейнер текстового редактора, и создаёт его при необходимости.</item>
    ///   <item>Генерирует уникальное имя файла (например, "Новый", "Новый1", "Новый2" и т.д.).</item>
    ///   <item>Создаёт новый экземпляр <see cref="TextEditorUI"/> и отображает его в рабочем пространстве.</item>
    ///   <item>Добавляет запись о новом файле в систему путей (<see cref="EditorWorkspaceModel.FilePaths"/>).</item>
    /// </list>
    /// </summary>
    public void CreateNewFile()
    {
      TextEditorContainer textEditorContainer = _fileManager.ContainerService.GetContainer(EditorType.TextEditor);

      if (textEditorContainer == null)
      {
        textEditorContainer = _fileManager.ContainerService.CreateContainer(EditorType.TextEditor);
      }

      var controlName = "Новый";
      var counter = 0;
      while (_fileManager.EditorWorkspaceModel.FilePaths.ContainsKey(controlName))
      {
        counter++;
        if (controlName != "Новый")
        {
          controlName = controlName.Remove(controlName.Length - (counter - 1).ToString().Length, (counter - 1).ToString().Length);
        }

        controlName += $"{counter}";
      }

      var textEditor = new TextEditorUI();

      var textEditorModel = new TextEditorModel(controlName);
      textEditor.TextEditorModel = textEditorModel;
      _fileManager.DockItemService.ShowNewDockItem(controlName, textEditorContainer, textEditor);
      _fileManager.EditorWorkspaceModel.FilePaths.Add(controlName, string.Empty);
    }
  }
}
