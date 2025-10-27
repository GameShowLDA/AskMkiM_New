using DTO.Enum;
using System.ComponentModel.DataAnnotations;

namespace DTO.SettingsModels
{
  public class UserInterfaceModel
  {
    [Key]
    public int Id { get; set; } = 1;

    /// <summary>
    /// Выбранный язык интерфейса программы.
    /// </summary>
    public string Language { get; set; }
    
    /// <summary>
    /// Выбранная тема оформления интерфейса программы.
    /// </summary>
    public ThemeEnums.Theme Theme { get; set; }

    /// <summary>
    /// Флаг, указывающий, нужно ли включить подсветку синтаксиса в редакторе.
    /// </summary>
    public bool UseSyntaxHighlighting { get; set; }
  }
}
