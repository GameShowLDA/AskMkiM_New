using System.Windows;
using UI.Controls.Archive.Models;

namespace UI.Components.Archive.ArchiveMenu
{
  public partial class ArchiveContextMenu : ResourceDictionary
  {
    public ArchiveContextMenu()
    {
      InitializeComponent();
    }

    private void OnOpenArchiveClick(object sender, RoutedEventArgs e)
    {
      if (GetSenderItem(sender) is ArchiveModel model)
        ArchiveContextMenuEvents.RaiseOpen(model);
    }

    private void OnRevealArchiveClick(object sender, RoutedEventArgs e)
    {
      if (GetSenderItem(sender) is ArchiveModel model)
        ArchiveContextMenuEvents.RaiseReveal(model);
    }

    private void OnDeleteArchiveClick(object sender, RoutedEventArgs e)
    {
      if (GetSenderItem(sender) is ArchiveModel model)
        ArchiveContextMenuEvents.RaiseDelete(model);
    }

    private static object? GetSenderItem(object sender)
    {
      if (sender is FrameworkElement fe && fe.DataContext is ArchiveModel m)
        return m;
      return null;
    }
  }

  public static class ArchiveContextMenuEvents
  {
    /// <summary>Вызывается при выборе пункта «Открыть архив».</summary>
    public static event EventHandler<ArchiveModel>? OpenArchiveRequested;

    /// <summary>Вызывается при выборе пункта «Показать в папке».</summary>
    public static event EventHandler<ArchiveModel>? RevealArchiveRequested;

    /// <summary>Вызывается при выборе пункта «Удалить архив».</summary>
    public static event EventHandler<ArchiveModel>? DeleteArchiveRequested;

    internal static void RaiseOpen(ArchiveModel model) =>
        OpenArchiveRequested?.Invoke(null, model);

    internal static void RaiseReveal(ArchiveModel model) =>
        RevealArchiveRequested?.Invoke(null, model);

    internal static void RaiseDelete(ArchiveModel model) =>
        DeleteArchiveRequested?.Invoke(null, model);
  }
}
