using Microsoft.Win32;
using System.IO;
using System.Windows;
using UI.Controls.Archive.Models;

namespace UI.Components.Archive
{
  public partial class NewArchiveDialog : Window
  {
    public NewArchiveDialog()
    {
      InitializeComponent();
    }

    /// <summary>
    /// Результат диалога: данные для создания архива.
    /// </summary>
    public NewArchiveModel Result { get; private set; } = new();

    private void Browse_Click(object sender, RoutedEventArgs e)
    {
      var dlg = new System.Windows.Forms.FolderBrowserDialog
      {
        Description = "Выберите папку для сохранения архива"
      };
      if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
      {
        PathBox.Text = dlg.SelectedPath;
      }
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
      if (string.IsNullOrWhiteSpace(PathBox.Text) || !Directory.Exists(PathBox.Text))
      {
        MessageBox.Show("Выберите папку для сохранения архива", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
        return;
      }

      if (string.IsNullOrWhiteSpace(NameBox.Text))
      {
        MessageBox.Show("Введите название архива", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
        return;
      }

      var archiveFile = Path.Combine(PathBox.Text, $"{NameBox.Text.Trim()}.apkw");

      Result = new NewArchiveModel
      {
        ArchiveName = NameBox.Text.Trim(),
        Description = DescBox.Text.Trim(),
        // можно хранить полный путь сразу:
        FullPath = archiveFile
      };

      DialogResult = true;
      Close();
    }
  }
}
