using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using DTO.Base.Models;
using UI.Components.ArchiveControls;
using UI.Components.ArchiveManager.Models;
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
    public ArchiveService(UI.Components.MultiEditorMethods.FileManager fileManager)
    {
      _fileManager = fileManager;
    }

    /// <summary>
    /// Асинхронно открывает интерфейс архивов и отображает таблицу со всеми доступными архивами.
    /// </summary>
    public async Task OpenArchiveBrowserAsync()
    {
      var container = GetOrCreateArchiveContainer();
      var archivesControl = await ShowArchiveListAsync(container);

      SubscribeToArchiveSelection(archivesControl);
      _fileManager.ControlManagerService.ShowEditorContainer(container, EditorType.Archive);
    }

    #region 📁 Открытие списка архивов

    /// <summary>
    /// Получает существующий контейнер архивов или создаёт новый.
    /// </summary>
    private TextEditorContainer GetOrCreateArchiveContainer()
    {
      return _fileManager.ContainerService.GetEditorContainer(EditorType.Archive)
          ?? _fileManager.ContainerService.CreateEditorContainer(EditorType.Archive);
    }

    /// <summary>
    /// Отображает вкладку со списком архивов, создавая её при необходимости.
    /// </summary>
    private async Task<TableAllArchivesControl> ShowArchiveListAsync(TextEditorContainer container)
    {
      var existingDockItem = container.DockManager.DockItems
          .FirstOrDefault(item => item.TabText == "Все архивы");

      if (existingDockItem?.Content is TableAllArchivesControl existingControl)
      {
        _fileManager.DockItemService.ShowDockItem(container, existingDockItem);
        return existingControl;
      }

      var newControl = new TableAllArchivesControl();
      _fileManager.DockItemService.ShowEditorDockItem("Все архивы", container, newControl);
      return newControl;
    }

    /// <summary>
    /// Подписывает сервис на событие выбора архива пользователем.
    /// </summary>
    private void SubscribeToArchiveSelection(TableAllArchivesControl archivesControl)
    {
      archivesControl.ArchiveSelected -= OnArchiveSelectedAsync;
      archivesControl.ArchiveSelected += OnArchiveSelectedAsync;
    }

    #endregion

    #region 📂 Обработка выбора архива

    /// <summary>
    /// Обрабатывает выбор архива пользователем и открывает его содержимое в новой вкладке.
    /// </summary>
    private async void OnArchiveSelectedAsync(object sender, MouseButtonEventArgs e)
    {
      if (TryGetSelectedArchive(e, out ApkArchive selectedArchive))
      {
        await OpenArchiveContentsAsync(selectedArchive);
      }
    }

    /// <summary>
    /// Извлекает выбранный архив из события мыши.
    /// </summary>
    private bool TryGetSelectedArchive(MouseButtonEventArgs e, out ApkArchive selectedArchive)
    {
      selectedArchive = null;
      if (e.Source is DataGrid grid && grid.SelectedItem is ApkArchive archive)
      {
        selectedArchive = archive;
        return true;
      }
      return false;
    }

    /// <summary>
    /// Открывает содержимое выбранного архива в новой вкладке.
    /// </summary>
    private Task OpenArchiveContentsAsync(ApkArchive archive)
    {
      var container = GetOrCreateArchiveContainer();
      _fileManager.DockItemService.ShowEditorDockItem(
          archive.ArchiveName,
          container,
          new TableApkArchiveControl(archive.ArchiveName));

      _fileManager.ControlManagerService.ShowEditorContainer(container, EditorType.Archive);
      return Task.CompletedTask;
    }

    #endregion

    #region 📄 Открытие OPK-файла

    /// <summary>
    /// Открывает OPK-файл в режиме только для чтения в контейнере архивов.
    /// </summary>
    public void OpenOpkFile(UserControl userControl, string elementName)
    {
      LogDebug($"Происходит открытие opk файла {elementName}.");

      var container = GetOrCreateArchiveContainer();
      if (userControl is TextEditorUI textEditor)
      {
        textEditor.IsReadOnly = true;
        _fileManager.DockItemService.ShowEditorDockItem(elementName, container, textEditor, EditorType.Archive);
        _fileManager.ControlManagerService.ShowEditorContainer(container, EditorType.Archive);
      }
    }

    #endregion
  }
}
