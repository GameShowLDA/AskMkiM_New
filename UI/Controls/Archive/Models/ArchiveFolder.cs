using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UI.Controls.Archive.Models
{
  /// <summary>
  /// Папка, содержащая архивы.
  /// </summary>
  public class ArchiveFolder
  {
    /// <summary>
    /// Название папки (отображаемое имя).
    /// </summary>
    public string FolderName { get; set; }

    /// <summary>
    /// Полный путь к папке.
    /// </summary>
    public string FolderPath { get; set; }

    /// <summary>
    /// Список архивов в папке.
    /// </summary>
    public List<ArchiveModel> Archives { get; set; } = new();

    /// <summary>Системный корень (только просмотр).</summary>
    public bool IsSystem { get; set; }   // ← добавили
  }
}
