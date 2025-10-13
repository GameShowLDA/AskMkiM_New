using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DataBaseConfiguration.Services;
using DTO.Base.Models;
using DTO.Base.Models.Session;
using DTO.Settings.SettingsModels;
using EventCore.Adapters;
using EventCore.Events;
using Message;
using Ude;
using UI.Components.ArchiveControls;
using UI.Components.ArchiveManager.Models;
using UI.Components.FileComparerControls;
using UI.Components.Invoke;
using UI.Controls;
using UI.Controls.Runner;
using UI.Controls.TextEditor;
using UI.Services;
using UI.Services.FileManager;
using UI.Services.ProtocolManager;
using UI.Services.Services;
using UI.Windows.WpfDocking.Windows.Docking;
using Utilities.Services;
using static DTO.Enum.FileEnums;
using static UI.Controls.TextEditor.TextEditorUI;
using static Utilities.LoggerUtility;
using Path = System.IO.Path;
using UserControl = System.Windows.Controls.UserControl;

namespace UI.Components.MultiEditorMethods
{
  /// <summary>
  /// Класс для работы с файлами.
  /// </summary>
  public class FileManager
  {
    /// <summary>
    /// Конструктор для инициализации файлового менеджера.
    /// </summary>
    /// <param name="multiEditorControl">Экземпляр класса MultiEditorControl.</param>
    public FileManager(MultiEditorControl multiEditorControl)
    {
      EditorWorkspaceModel = new EditorWorkspaceModel(multiEditorControl);
      ArchiveService = new ArchiveService(this);
      ContainerService = new ContainerService(this);
      ProtocolService = new ProtocolService(this);
      ControlManagerService = new ControlManagerService(EditorWorkspaceModel);
      DockItemService = new DockItemService(this);
      FolderService = new FolderService(this);
      RunControlService = new RunControlService(this);
      TextEditorService = new TextEditorService(this);
      TranslationService = new TranslationService(this);
      FileService = new FileService(this);
      SessionService = new SessionManager(this);
    }

    public EditorWorkspaceModel EditorWorkspaceModel;
    public FileService FileService;
    public ArchiveService ArchiveService;
    public ContainerService ContainerService;
    public ProtocolService ProtocolService;
    public ControlManagerService ControlManagerService;
    public DockItemService DockItemService;
    public FolderService FolderService;
    public RunControlService RunControlService;
    public TextEditorService TextEditorService;
    public TranslationService TranslationService;
    public SessionManager SessionService;
  }
}
