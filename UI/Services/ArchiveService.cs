using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using DTO.Base.Models;
using UI.Components.ArchiveControls;
using UI.Components.ArchiveManager.Models;
using UI.Components.MultiEditorMethods;
using UI.Controls.TextEditor;
using static Utilities.LoggerUtility;

namespace UI.Services
{
  /// <summary>
  /// Сервис управления архивами в интерфейсе приложения.
  /// 
  /// Основные задачи:
  /// <list type="bullet">
  ///   <item>Открытие панели со списком архивов и отображение их содержимого.</item>
  ///   <item>Обработка выбора конкретного архива и открытие его структуры.</item>
  ///   <item>Открытие файлов OPK в режиме только для чтения.</item>
  /// </list>
  /// </summary>
  public class ArchiveService
  {
    private readonly UI.Components.MultiEditorMethods.FileManager _fileManager;

    /// <summary>
    /// Создаёт новый экземпляр сервиса для работы с архивами.
    /// </summary>
    /// <param name="fileManager">Главный файловый менеджер, предоставляющий доступ к контейнерам и вкладкам редактора.</param>
    public ArchiveService(UI.Components.MultiEditorMethods.FileManager fileManager)
    {
      _fileManager = fileManager;
    }

    /// <summary>
    /// Асинхронно открывает интерфейс архивов и отображает таблицу со всеми доступными архивами.
    /// 
    /// Если вкладка со списком архивов уже существует — активирует её.  
    /// Если нет — создаёт новую вкладку с таблицей архивов.
    /// </summary>
    public async Task OpenArchiveBrowserAsync()
    {
      TextEditorContainer archiveContainer = _fileManager.ContainerService.GetContainer(EditorType.Archive);
      TableAllArchivesControl allArchives = null;
      if (archiveContainer == null)
      {
        archiveContainer = _fileManager.ContainerService.CreateContainer(EditorType.Archive);
      }
      if (archiveContainer.DockManager.DockItems.FirstOrDefault(item => item.TabText == "Все архивы") != null)
      {
        var foundDockItem = archiveContainer.DockManager.DockItems.FirstOrDefault(item => item.TabText == "Все архивы");
        if (foundDockItem.Content is TableAllArchivesControl archivesTable)
        {
          allArchives = archivesTable;
          _fileManager.DockItemService.ShowDockItem(archiveContainer, foundDockItem);
        }
      }
      else
      {
        allArchives = new TableAllArchivesControl();
        _fileManager.DockItemService.ShowNewDockItem("Все архивы", archiveContainer, allArchives);
      }

      _fileManager.ControlManagerService.ShowControl(archiveContainer, EditorType.Archive);
      allArchives.ArchiveSelected -= OnArchiveSelectedAsync;
      allArchives.ArchiveSelected += OnArchiveSelectedAsync;
    }

    /// <summary>
    /// Обрабатывает выбор архива пользователем в таблице.  
    /// Загружает и отображает содержимое выбранного архива в новой вкладке.
    /// </summary>
    private async void OnArchiveSelectedAsync(object sender, MouseButtonEventArgs e)
    {
      var dataGrid = e.Source as DataGrid;
      if (dataGrid?.SelectedItem is ApkArchive selectedArchive)
      {
        if (selectedArchive != null)
        {
          TextEditorContainer archiveContainer = _fileManager.ContainerService.GetContainer(EditorType.Archive);
          var archiveName = selectedArchive.ArchiveName;
          _fileManager.DockItemService.ShowNewDockItem(archiveName, archiveContainer, new TableApkArchiveControl(archiveName));
          _fileManager.ControlManagerService.ShowControl(archiveContainer, EditorType.Archive);
        }
      }
    }

    /// <summary>
    /// Открывает OPK-файл в режиме только для чтения в контейнере архивов.
    /// </summary>
    /// <param name="userControl">Контрол, содержащий содержимое OPK-файла.</param>
    /// <param name="elementName">Имя файла или элемента, которое будет отображаться на вкладке.</param>
    public void OpenOpkFile(UserControl userControl, string elementName)
    {
      LogDebug($"Происходит открытие opk файла {elementName}.");
      TextEditorContainer archiveContainer = _fileManager.ContainerService.GetContainer(EditorType.Archive);
      var textEditor = userControl as TextEditorUI;
      textEditor.IsReadOnly = true;
      _fileManager.DockItemService.ShowNewDockItem(elementName, archiveContainer, textEditor, EditorType.Archive);
      _fileManager.ControlManagerService.ShowControl(archiveContainer, EditorType.Archive);
    }
  }
}
