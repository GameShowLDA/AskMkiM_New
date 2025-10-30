using DTO.Attributes;
using DTO.Attributes.Attributes;
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
    public static CommandOrganizationalAttribute? GetDisplayOrganizationalInfo(this Enum value)
    {
      var member = value.GetType().GetMember(value.ToString()).FirstOrDefault();
      return member?.GetCustomAttribute<CommandOrganizationalAttribute>();
    }

    public static bool MatchesEnum(this string mnemonic, Enum value)
    {
      var display = value.GetDisplayInfo() ?? (object?)value.GetDisplayOrganizationalInfo();
      var displayMnemonic =
          display switch
          {
            CommandDisplayInfoAttribute info => info.DisplayName,
            CommandOrganizationalAttribute org => org.DisplayName,
            _ => value.ToString()
          };

      return string.Equals(mnemonic, displayMnemonic, StringComparison.OrdinalIgnoreCase);
    }
  }
}
