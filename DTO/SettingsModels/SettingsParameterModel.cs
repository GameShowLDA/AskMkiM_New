using DTO.Enum;

namespace DTO.SettingsModels
{
  public class SettingsParameterModel
  {
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
