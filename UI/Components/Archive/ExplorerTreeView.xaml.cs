using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UI.Controls.Archive.Models;
using UI.Components.Archive.ArchiveMenu;

namespace UI.Components.Archive
{
  /// <summary>
  /// Пользовательский элемент управления для отображения дерева архивов.
  /// Включает поддержку:
  /// <list type="bullet">
  /// <item>отображения коллекции папок и архивов;</item>
  /// <item>поддержки контекстных меню для различных типов элементов;</item>
  /// <item>автоматического выделения элемента при клике ПКМ;</item>
  /// <item>проброса событий в основной контроллер (<see cref="ArchiveControlManager"/>).</item>
  /// </list>
  /// </summary>
  public partial class ExplorerTreeView : UserControl
  {

    /// <summary>
    /// Создаёт новый экземпляр <see cref="ExplorerTreeView"/>.
    /// Подписывается на события <see cref="ArchiveContextMenuEvents"/> и пробрасывает их наружу.
    /// </summary>
    public ExplorerTreeView()
    {
      InitializeComponent();

      // Подписка на события ArchiveContextMenu
      ArchiveContextMenuEvents.OpenArchiveRequested += (s, m) =>
        OpenArchiveRequested?.Invoke(this, m);

      ArchiveContextMenuEvents.RevealArchiveRequested += (s, m) =>
        RevealArchiveRequested?.Invoke(this, m);

      ArchiveContextMenuEvents.DeleteArchiveRequested += (s, m) =>
        DeleteArchiveRequested?.Invoke(this, m);

      AppConfiguration.Base.EventAggregator.AdminRightsChanged += OnAdminRightsChanged;

      Loaded += async (_, __) =>
      {
        var isAdmin = await AppConfiguration.Admin.AdminConfig.GetAdminRights();
        IsAdmin = isAdmin;
      };
    }

    // ===================== Dependency Properties =====================

    /// <summary>
    /// Источник данных для отображения в <see cref="TreeView"/>.
    /// Обычно это коллекция <see cref="ArchiveFolder"/> с вложенными
    /// <see cref="ArchiveModel"/> и <see cref="OpkModel"/>.
    /// </summary>
    public static readonly DependencyProperty ItemsSourceProperty =
      DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable),
        typeof(ExplorerTreeView), new PropertyMetadata(null));

    /// <inheritdoc cref="ItemsSourceProperty"/>
    public IEnumerable? ItemsSource
    {
      get => (IEnumerable?)GetValue(ItemsSourceProperty);
      set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    /// Признак административных прав.
    /// Если <c>true</c>, в XAML-триггерах включаются возможности редактирования системных пакетов
    /// (создание архивов, удаление и т. п.).
    /// </summary>
    public static readonly DependencyProperty IsAdminProperty =
         DependencyProperty.Register(nameof(IsAdmin), typeof(bool),
             typeof(ExplorerTreeView), new PropertyMetadata(false));

    /// <inheritdoc cref="IsAdminProperty"/>
    public bool IsAdmin
    {
      get => (bool)GetValue(IsAdminProperty);
      private set => SetValue(IsAdminProperty, value);
    }

    // ===================== Events =====================

    /// <summary>Событие: пользователь выбрал создание корневой папки архивов.</summary>
    public event EventHandler? CreateRootRequested;

    /// <summary>Событие: пользователь открыл папку архивов.</summary>
    public event EventHandler<ArchiveFolder>? OpenFolderRequested;

    /// <summary>Событие: пользователь запросил показать папку в проводнике.</summary>
    public event EventHandler<ArchiveFolder>? RevealFolderRequested;

    /// <summary>Событие: пользователь запросил создание нового архива в выбранной папке.</summary>
    public event EventHandler<ArchiveFolder>? CreateArchiveInFolderRequested;

    /// <summary>Событие: пользователь удалил выбранный корень архивов.</summary>
    public event EventHandler<ArchiveFolder>? DeleteRootRequested;

    /// <summary>Событие: пользователь открыл архив (.apkw).</summary>
    public event EventHandler<ArchiveModel>? OpenArchiveRequested;

    /// <summary>Событие: пользователь запросил показать архив в проводнике.</summary>
    public event EventHandler<ArchiveModel>? RevealArchiveRequested;

    /// <summary>Событие: пользователь удалил архив (.apkw).</summary>
    public event EventHandler<ArchiveModel>? DeleteArchiveRequested;

    /// <summary>Событие: пользователь запросил показать родительский архив для OPK-файла.</summary>
    public event EventHandler<OpkModel>? RevealArchiveFromOpkRequested;

    /// <summary>Событие: пользователь удалил OPK-файл из архива.</summary>
    public event EventHandler<OpkModel>? DeleteOpkRequested;

    // ===================== Context Menu Handlers =====================
    // Здесь оставляем только для папок и OPK — архивные пробрасываются через ArchiveContextMenuEvents.
    private void OnCreateRootClick(object sender, RoutedEventArgs e) =>
        CreateRootRequested?.Invoke(this, EventArgs.Empty);

    private void OnOpenFolderClick(object sender, RoutedEventArgs e)
    {
      if (PART_Tree.SelectedItem is ArchiveFolder folder)
        OpenFolderRequested?.Invoke(this, folder);
    }

    private void OnRevealFolderClick(object sender, RoutedEventArgs e)
    {
      if (PART_Tree.SelectedItem is ArchiveFolder folder)
        RevealFolderRequested?.Invoke(this, folder);
    }

    private void OnCreateArchiveClick(object sender, RoutedEventArgs e)
    {
      if (PART_Tree.SelectedItem is ArchiveFolder folder)
        CreateArchiveInFolderRequested?.Invoke(this, folder);
    }

    private void OnDeleteRootClick(object sender, RoutedEventArgs e)
    {
      if (PART_Tree.SelectedItem is ArchiveFolder folder)
        DeleteRootRequested?.Invoke(this, folder);
    }

    private void OnRevealArchiveFromOpkClick(object sender, RoutedEventArgs e)
    {
      if (PART_Tree.SelectedItem is OpkModel opk)
        RevealArchiveFromOpkRequested?.Invoke(this, opk);
    }

    private void OnDeleteOpkClick(object sender, RoutedEventArgs e)
    {
      if (PART_Tree.SelectedItem is OpkModel opk)
        DeleteOpkRequested?.Invoke(this, opk);
    }

    private void OnAdminRightsChanged(bool isAdmin)
    {
      Dispatcher.Invoke(() => IsAdmin = isAdmin);
    }

    // ===================== Mouse Behavior =====================

    /// <summary>
    /// Обработчик нажатия правой кнопки мыши.
    /// Этот приём необходим, так как в WPF <see cref="TreeView"/> элемент под курсором
    /// не выделяется автоматически при ПКМ.
    /// Мы вручную выделяем <see cref="TreeViewItem"/>, чтобы его DataContext
    /// корректно использовался в связанной команде или контекстном меню.
    /// </summary>
    private void Tree_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
      var src = e.OriginalSource as DependencyObject;
      var tvi = src != null
        ? ItemsControl.ContainerFromElement((ItemsControl)sender, src) as TreeViewItem
        : null;

      if (tvi != null)
      {
        tvi.IsSelected = true;
        tvi.Focus();
      }
    }
  }
}
