using ControlCommandAnalyser.Model;
using Errors.Translation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Utilities;

namespace ControlCommandAnalyser.Parser
{
  public static class KeyParser
  {
    public static string ParseKeys(int numberLine, BaseCommandModel model, string remainder)
    {
      var result = AlgorithmKeyParser.ExtractKeysWithTrailingCommaCheck(remainder, model);

      foreach (var (key, hasError) in result)
      {
        if (hasError)
        {
          model.Errors.Add(GeneralErrors.WrongKey(numberLine, model.Mnemonic, $"{model.CommandNumber} {model.Mnemonic}", key));
        }
        else
        {
          if (!model.AlgorithmKey.Contains(key))
          {
            model.AlgorithmKey.Add(key);
            LoggerUtility.LogDebug($"Найден ключ алгоритма: {key}");
          }
          else
          {
            model.Warnings.Add(GeneralWarnings.DuplicateKey(numberLine,$"{model.CommandNumber} {model.Mnemonic}", key));
          }
        }
      }

      // удаляем найденные ключи ТОЛЬКО из ПИ-остатка
      foreach (var (key, hasError) in result)
      {
        remainder = Regex.Replace(
        remainder,
        $@"\b{Regex.Escape(key)}\s*,?",
        "",
        RegexOptions.IgnoreCase);
      }

      return remainder;
    }
  }
}
