using DTO.Base.Models;
using EventCore.Adapters;
using Message;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UI.Controls.TextEditor;
using Utilities.Services;
using static DTO.Enum.FileEnums;
using static Utilities.LoggerUtility;

namespace UI.Services.FileManager
{
  /// <summary>
  /// Сервис для открытия файлов в текстовом редакторе.
  /// 
  /// Основные функции:
  /// <list type="bullet">
  ///   <item>Открывает указанный файл и загружает его содержимое в редактор.</item>
  ///   <item>Определяет кодировку файла и тип содержимого по расширению.</item>
  ///   <item>Создаёт новый редактор для файла или активирует уже открытый экземпляр.</item>
  ///   <item>Обрабатывает ошибки чтения и уведомляет пользователя в случае сбоев.</item>
  /// </list>
  /// </summary>
  public class FileOpenService
  {
    private readonly UI.Components.MultiEditorMethods.FileManager _fileManager;

    /// <summary>
    /// Создаёт новый экземпляр сервиса открытия файлов.
    /// </summary>
    /// <param name="fileManager">Главный файловый менеджер, предоставляющий доступ к редактору и контейнерам.</param>
    public FileOpenService(UI.Components.MultiEditorMethods.FileManager fileManager)
    {
      _fileManager = fileManager;
    }

    /// <summary>
    /// Открывает файл по указанному пути и отображает его содержимое в текстовом редакторе.
    /// 
    /// Если файл с таким именем уже открыт, то активируется существующая вкладка.
    /// Если файл отсутствует или не может быть прочитан, пользователю будет показано сообщение об ошибке.
    /// </summary>
    /// <param name="path">Полный путь к файлу.</param>
    public void OpenFile(string path)
    {
      Application.Current.Dispatcher.BeginInvoke(() =>
      {
        var nameFile = ExtractFileName(path);
        if (string.IsNullOrEmpty(nameFile))
        {
          MessageBoxCustom.Show("Ошибка при открытии файла", $"Ошибка при открытии файла {path}", image: MessageBoxImage.Error);
          return;
        }

        try
        {
          string fileContent = string.Empty;
          var fileData = ReadFileContent(path).ToTuple();
          fileContent = fileData.Item1;
          var encoding = fileData.Item2;
          TextEditorContainer textEditorContainer = _fileManager.ContainerService.GetContainer(EditorType.TextEditor);
          if (textEditorContainer == null)
          {
            textEditorContainer = _fileManager.ContainerService.CreateContainer(EditorType.TextEditor);
          }

          var fileType = DetermineFileType(nameFile);
          if (_fileManager.EditorWorkspaceModel.FilePaths.ContainsValue(path))
          {
            var existingItem = textEditorContainer.DockManager.DockItems.FirstOrDefault(item => item.TabText == nameFile);
            if (existingItem != null)
            {
              _fileManager.DockItemService.ShowDockItem(textEditorContainer, existingItem);
              _fileManager.ControlManagerService.ShowControl(textEditorContainer, EditorType.TextEditor);
              return;
            }
          }
          var newFileName = _fileManager.FileService.Name.EnsureUniqueFileName(path, nameFile);

          var textEditorModel = new TextEditorModel(path, newFileName, encoding);
          var textEditor = _fileManager.TextEditorService.CreateTextEditor(textEditorModel, fileContent, fileType);
          if (fileType == FileType.Protocol)
          {
            textEditor.IsReadOnly = true;
          }

          EditorEventAdapter.RaiseTextEditorActivated(textEditor);
          _fileManager.DockItemService.ShowNewDockItem(newFileName, textEditorContainer, textEditor);

          _fileManager.ControlManagerService.ShowControl(textEditorContainer, EditorType.TextEditor);
        }
        catch (Exception ex)
        {
          MessageBoxCustom.Show($"Ошибка при чтении файла: {ex.Message}", "Ошибка", image: MessageBoxImage.Error);
          LogException($"Ошибка при чтении файла", ex);
        }
      });
    }

    /// <summary>
    /// Считывает содержимое указанного файла и определяет его кодировку.
    /// </summary>
    /// <param name="path">Путь к файлу.</param>
    /// <returns>Кортеж, содержащий содержимое файла в виде строки и объект <see cref="Encoding"/>.</returns>
    private (string, Encoding) ReadFileContent(string path)
    {
      Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

      Encoding encoding = EncodingService.DetectEncodingFromFile(path);

      var content = new List<string>();
      foreach (string line in File.ReadLines(path, encoding))
      {
        if (!string.IsNullOrEmpty(line))
        {
          content.Add(line);
        }
      }

      var fileContent = content.Count > 0 ? string.Join("\n", content) : string.Empty;

      return (fileContent, encoding);
    }

    /// <summary>
    /// Определяет тип файла по его расширению.
    /// </summary>
    /// <param name="fileName">Имя файла с расширением.</param>
    /// <returns>Тип файла из перечисления <see cref="FileType"/>.</returns>
    public FileType DetermineFileType(string fileName)
    {
      if (string.IsNullOrEmpty(fileName))
        return FileType.None;
      var ext = Path.GetExtension(fileName).ToLowerInvariant();
      return ext switch
      {
        ".pk" => FileType.PK,
        ".pkw" => FileType.PKW,
        ".opk" => FileType.OPK,
        ".opkw" => FileType.OPKW,
        ".lst" => FileType.Protocol,
        ".lstw" => FileType.Protocol,
        _ => FileType.None
      };
    }

    /// <summary>
    /// Извлекает имя файла из полного пути.
    /// </summary>
    /// <param name="path">Полный путь к файлу.</param>
    /// <returns>Имя файла с расширением или пустую строку, если путь некорректен.</returns>
    private string ExtractFileName(string path)
    {
      return string.IsNullOrEmpty(path) ? string.Empty : System.IO.Path.GetFileName(path);
    }
  }
}
