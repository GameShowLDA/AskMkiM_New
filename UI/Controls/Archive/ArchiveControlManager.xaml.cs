using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using AppConfiguration;
using EventCore.Events;
using EventCore.Services;
using UI.Components.Archive;
using UI.Controls.Archive.Models;
using UI.Controls.Archive.Services;

namespace UI.Controls.Archive
{
  /// <summary>
  /// Управляющий элемент для работы с архивами (проводник).
  /// Содержит панель с заголовком и дерево <see cref="ExplorerTreeView"/>.
  /// Подписывается на события дерева и выполняет действия:
  /// <list type="bullet">
  /// <item>создание и удаление корневых папок;</item>
  /// <item>создание архивов (системных и пользовательских);</item>
  /// <item>открытие и удаление архивов;</item>
  /// <item>открытие папок и файлов в проводнике.</item>
  /// </list>
  /// </summary>
  public partial class ArchiveControlManager : UserControl
  {
    /// <summary>
    /// Признак административных прав.
    /// Используется для включения/отключения функций редактирования системных пакетов.
    /// </summary>
    public static readonly DependencyProperty IsAdminProperty =
      DependencyProperty.Register(nameof(IsAdmin), typeof(bool),
        typeof(ArchiveControlManager), new PropertyMetadata(false));

    /// <inheritdoc cref="IsAdminProperty"/>
    public bool IsAdmin
    {
      get => (bool)GetValue(IsAdminProperty);
      set => SetValue(IsAdminProperty, value);
    }

    /// <summary>
    /// Создаёт новый экземпляр <see cref="ArchiveControlManager"/>.
    /// Подписывается на события <see cref="ExplorerTreeView"/>.
    /// </summary>
    public ArchiveControlManager()
    {
      InitializeComponent();
      Loaded += OnLoadedAsync;

      // Подписка на события ExplorerTreeView
      Explorer.CreateRootRequested += async (_, __) => await CmdCreateRootAsync();
      Explorer.OpenFolderRequested += (_, folder) => OpenFolder(folder.FolderPath);
      Explorer.RevealFolderRequested += (_, folder) => ShowInExplorer(folder.FolderPath);
      Explorer.CreateArchiveInFolderRequested += async (_, folder) => await CmdCreateArchiveInFolderAsync(folder);
      Explorer.DeleteRootRequested += async (_, folder) => await CmdRemoveRootAsync(folder);
      Explorer.OpenArchiveRequested += (_, archive) => OpenFile(archive.ArchivePath);
      Explorer.RevealArchiveRequested += (_, archive) => ShowInExplorer(archive.ArchivePath, true);
      Explorer.DeleteArchiveRequested += async (_, archive) => await CmdDeleteArchiveAsync(archive);
      Explorer.RevealArchiveFromOpkRequested += (_, opk) =>
      {
        // TODO: Реализовать получение родительского архива для OPK
        MessageBox.Show($"OPK-файл: {opk.OpkFilename}", "Показать архив");
      };
      Explorer.DeleteOpkRequested += async (_, opk) =>
      {
        // TODO: Реализовать удаление OPK-файла
        MessageBox.Show($"Удалить OPK: {opk.OpkFilename}", "Удаление");
        await RefreshAsync();
      };
    }

    /// <summary>
    /// Загрузка данных при открытии контрола.
    /// </summary>
    private async void OnLoadedAsync(object? sender, RoutedEventArgs e)
    {
      await RefreshAsync();

      var a = await AdminConfig.GetAdminRights().ConfigureAwait(false);
      await Dispatcher.InvokeAsync(() => IsAdmin = a);

      EventAggregator.Subscribe<SystemStateEvents.AdminRightsChanged>(e =>
      {
        if (!Dispatcher.CheckAccess())
        {
          Dispatcher.BeginInvoke(new Action(() => IsAdmin = e.IsAdmin));
        }
        else
        {
          IsAdmin = e.IsAdmin;
        }
      });

    }

    /// <summary>
    /// Обновляет дерево архивов.
    /// </summary>
    private async Task RefreshAsync()
    {
      var svc = new ArchiveTreeService();
      var items = await svc.BuildTreeAsyncCombined();
      Explorer.ItemsSource = items;
    }

    /// <summary>
    /// Создание нового корня архивов.
    /// </summary>
    private async Task CmdCreateRootAsync()
    {
      var dlg = new System.Windows.Forms.FolderBrowserDialog { Description = "Выберите папку с .apkw архивами" };
      if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
      {
        var name = Path.GetFileName(dlg.SelectedPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        await UserArchiveRootsUiService.AddOrUpdateAsync(name, dlg.SelectedPath, recursive: true);
        await RefreshAsync();
      }
    }

    /// <summary>
    /// Создание нового архива в указанной папке.
    /// </summary>
    private async Task CmdCreateArchiveInFolderAsync(ArchiveFolder folder)
    {
      var dlg = new NewArchiveDialog { Owner = Window.GetWindow(this) };
      if (dlg.ShowDialog() != true) return;

      var info = dlg.Result;

      if (folder.IsSystem)
      {
        // await SystemArchivesService.CreateEmpty(info.FullPath, info);
      }
      else
      {
        await UserArchivesService.CreateEmpty(info.FullPath, info);
      }

      await RefreshAsync();
    }

    /// <summary>
    /// Удаление корневой папки архивов.
    /// </summary>
    private async Task CmdRemoveRootAsync(ArchiveFolder folder)
    {
      var roots = await UserArchiveRootsUiService.ListAsync();
      var ent = roots.Find(x =>
        string.Equals(Path.GetFullPath(x.FolderPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                      Path.GetFullPath(folder.FolderPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                      StringComparison.OrdinalIgnoreCase));
      if (ent != null)
      {
        await UserArchiveRootsUiService.RemoveByIdAsync(ent.Id);
        await RefreshAsync();
      }
    }

    /// <summary>
    /// Удаление архива (.apkw).
    /// </summary>
    private async Task CmdDeleteArchiveAsync(ArchiveModel a)
    {
      if (!string.IsNullOrWhiteSpace(a.ArchivePath) && File.Exists(a.ArchivePath))
      {
        UserArchivesService.Delete(a.ArchivePath);
        await RefreshAsync();
      }
    }

    /// <summary>
    /// Открывает папку в проводнике.
    /// </summary>
    private static void OpenFolder(string? path)
    {
      if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path)) return;
      Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
    }

    /// <summary>
    /// Открывает файл в связанной программе.
    /// </summary>
    private static void OpenFile(string? path)
    {
      if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return;
      Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
    }

    /// <summary>
    /// Показывает файл или папку в проводнике.
    /// </summary>
    private static void ShowInExplorer(string? path, bool select = false)
    {
      if (string.IsNullOrWhiteSpace(path)) return;
      if (select && File.Exists(path))
      {
        Process.Start(new ProcessStartInfo { FileName = "explorer.exe", Arguments = $"/select,\"{path}\"", UseShellExecute = true });
      }
      else if (Directory.Exists(path))
      {
        Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
      }
      else
      {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
          Process.Start(new ProcessStartInfo { FileName = dir, UseShellExecute = true });
      }
    }
  }
}
