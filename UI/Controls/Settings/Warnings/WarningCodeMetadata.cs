using Errors.Models;
using System.Reflection;

namespace UI.Controls.Settings.Warnings
{
  internal static class WarningCodeMetadata
  {
    public static IReadOnlyList<WarningSetting> ExtractDefaults()
    {
      return Enum.GetValues(typeof(WarningCode))
        .Cast<WarningCode>()
        .Select(code =>
        {
          var member = typeof(WarningCode)
            .GetMember(code.ToString())
            .First();

          return new WarningSetting
          {
            Code = code,
            Tag = code.GetTag() ?? code.ToString(),
            Title = GetSummary(member) ?? code.ToString(),
            IsEnabled = true
          };
        })
        .ToList();
    }

    private static string? GetSummary(MemberInfo member)
    {
      // если позже захочешь — можно читать XML-документацию
      return member.Name;
    }
  }
}
