using System;
using System.ComponentModel.DataAnnotations;

namespace DTO.Enum
{
  /// <summary>
  /// Содержит перечисление доступных тем оформления интерфейса приложения.
  /// </summary>
  public static class ThemeEnums
  {
    /// <summary>
    /// Перечисление тем оформления интерфейса.
    /// </summary>
    public enum Theme
    {
      /// <summary>
      /// Светлая тема интерфейса.
      /// </summary>
      [Display(Name = "Светлая тема")]
      Light = 1,

      /// <summary>
      /// Тёмная тема интерфейса.
      /// </summary>
      [Display(Name = "Тёмная тема")]
      Dark = 0,
    }
  }
}
