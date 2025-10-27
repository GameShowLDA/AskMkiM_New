using DTO.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
  public static class EnumExtensions
  {
    public static CommandDisplayInfoAttribute? GetDisplayInfo(this Enum value)
    {
      var member = value.GetType().GetMember(value.ToString()).FirstOrDefault();
      return member?.GetCustomAttribute<CommandDisplayInfoAttribute>();
    }
  }
}
