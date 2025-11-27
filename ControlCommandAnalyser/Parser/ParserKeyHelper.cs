using ControlCommandAnalyser.Attributes;
using DTO.Enum;

namespace ControlCommandAnalyser.Parser
{
  public static class ParserKeyHelper
  {
    public static HashSet<TranslationKey.AlgorithmKey> GetAllowedKeys(ICommandParser parser)
    {
      var attr = parser.GetType()
                       .GetCustomAttributes(typeof(AllowedKeysAttribute), false)
                       .FirstOrDefault() as AllowedKeysAttribute;

      return attr != null
          ? new HashSet<TranslationKey.AlgorithmKey>(attr.Keys)
          : new HashSet<TranslationKey.AlgorithmKey>();
    }
  }
}
