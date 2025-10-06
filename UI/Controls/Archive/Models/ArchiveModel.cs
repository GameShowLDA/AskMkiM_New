using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UI.Controls.Archive.Models
{
  /// <summary>
  /// Архив, содержащий OPK-файлы.
  /// </summary>
  public class ArchiveModel
  {
    /// <summary>
    /// Название архива.
    /// </summary>
    public string ArchiveName { get; set; }

    /// <summary>
    /// Примечание к архиву.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Признак, что архив принадлежит системной папке.
    /// Используется для скрытия/отключения контекстного меню.
    /// </summary>
    public bool IsSystem { get; set; }

    /// <summary>
    /// Список OPK-файлов внутри архива.
    /// </summary>
    public List<OpkModel> OpkFiles { get; set; } = new();

    /// <summary>Полный путь к файлу .apkw (для операций ПКМ).</summary>
    public string? ArchivePath { get; set; }
  }
}
