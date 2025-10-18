using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Extensions
{
  public static class EnumExtensions
  {
    /// <summary>
    /// Получает локализованное отображаемое имя для значения перечисления (Display.Name).
    /// </summary>
    public static string GetDisplayName(this Enum enumValue)
    {
      return enumValue.GetType()
          .GetMember(enumValue.ToString())
          .First()
          .GetCustomAttribute<DisplayAttribute>()?.Name ?? enumValue.ToString();
    }
  }
}
