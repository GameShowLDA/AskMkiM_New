namespace UI.Controls.Archive.Models
{
  /// <summary>
  /// Данные для создания нового архива.
  /// </summary>
  public class NewArchiveModel
  {
    /// <summary>Название архива.</summary>
    public string ArchiveName { get; set; } = string.Empty;

    /// <summary>Описание архива.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Полный путь к файлу архива (.apkw).</summary>
    public string FullPath { get; set; } = string.Empty;
  }
}
