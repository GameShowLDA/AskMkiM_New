using DTO.Base.Models;
using DTO.Settings.SettingsModels;
using EventCore.Adapters;
using Message;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UI.Controls.TextEditor;
using static DTO.Enum.FileEnums;
using static Utilities.LoggerUtility;

namespace UI.Services.ProtocolManager
{
  /// <summary>
  /// Сервис управления протоколами испытаний.
  /// 
  /// Основные задачи:
  /// <list type="bullet">
  ///   <item>Формирование текстового представления протокола с ошибками или без них.</item>
  ///   <item>Отображение протокола в интерфейсе программы или экспорт в PDF-файл.</item>
  ///   <item>Создание и открытие вкладки редактора с протоколом.</item>
  /// </list>
  /// </summary>
  public class ProtocolService
  {
    private readonly Components.MultiEditorMethods.FileManager _fileManager;

    /// <summary>
    /// Инициализирует новый экземпляр сервиса управления протоколами.
    /// </summary>
    /// <param name="fileManager">Главный файловый менеджер, необходимый для работы с контейнерами и редакторами.</param>
    public ProtocolService(Components.MultiEditorMethods.FileManager fileManager)
    {
      _fileManager = fileManager;
    }

    /// <summary>
    /// Отображает протокол проверки, сформированный по данным <see cref="ProtocolModel"/>.
    /// 
    /// В зависимости от параметра <paramref name="showInSoftware"/> протокол будет открыт в интерфейсе программы
    /// или сохранён и открыт в формате PDF.
    /// </summary>
    /// <param name="protocol">Модель протокола, содержащая результаты проверки.</param>
    /// <param name="showInSoftware">
    /// Если <c>true</c> — протокол откроется внутри приложения.  
    /// Если <c>false</c> — будет сформирован PDF-документ.
    /// </param>
    public void DisplayProtocol(ProtocolModel protocol, bool showInSoftware)
    {
      var protocolText = string.Empty;
      if (protocol.Errors.Count > 0)
      {
        protocolText = ProtocolModel.GetProtocolWithErrorsText(protocol);
      }
      else
      {
        protocolText = ProtocolModel.GetProtocolText(protocol);
      }
      if (!string.IsNullOrEmpty(protocolText))
      {
        if (showInSoftware == true)
        {
          OpenProtocolInEditor(protocol, protocolText);
        }
        else
        {
          ExportProtocolAsPdf(protocol.ProgramName, protocolText);
        }
      }
    }

    /// <summary>
    /// Сохраняет протокол в формате PDF и открывает его в стандартном приложении.
    /// </summary>
    public void ExportProtocolAsPdf(string programName, string protocolText)
    {
      var generator = new PdfProtocolGenerator();
      generator.GenerateAndSavePdfProtocol(programName, protocolText);
    }

    /// <summary>
    /// Открывает протокол внутри интерфейса программы в виде вкладки текстового редактора.
    /// </summary>
    /// <param name="protocol">Модель протокола, содержащая метаданные (имя программы, путь и т.д.).</param>
    /// <param name="protocolText">Содержимое протокола.</param>
    private void OpenProtocolInEditor(ProtocolModel protocol, string? protocolText)
    {
      Application.Current.Dispatcher.BeginInvoke(() =>
      {
        try
        {
          var containerType = EditorType.Protocol;
          TextEditorContainer protocolContainer = _fileManager.ContainerService.GetContainer(containerType);
          if (protocolContainer == null)
          {
            protocolContainer = _fileManager.ContainerService.CreateContainer(containerType);
          }

          var newFileName = $"{Path.GetFileNameWithoutExtension(protocol.ProgramName)} от {DateTime.Now:dd-mm-yyyy HH-mm-ss}.lstw";
          var newPath = Path.Combine(Path.GetDirectoryName(protocol.ProgramPath), newFileName);
          var textEditorModel = new TextEditorModel(newPath);
          var textEditor = _fileManager.TextEditorService.CreateTextEditor(textEditorModel, protocolText, FileType.Protocol);
          textEditor.IsReadOnly = true;

          EditorEventAdapter.RaiseTextEditorActivated(textEditor);

          _fileManager.DockItemService.ShowNewDockItem(newFileName, protocolContainer, textEditor, containerType);
          _fileManager.ControlManagerService.ShowControl(protocolContainer, containerType);
        }
        catch (Exception ex)
        {
          MessageBoxCustom.Show($"Ошибка при чтении файла: {ex.Message}", "Ошибка", image: MessageBoxImage.Error);
          LogException($"Ошибка при чтении файла", ex);
        }
      });
    }
  }
}
