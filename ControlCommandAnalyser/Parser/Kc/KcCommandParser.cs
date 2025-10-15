using System.Text.RegularExpressions;
using AppConfiguration.Error.Translation;
using ControlCommandAnalyser.Model;
using ControlCommandAnalyser.Model.Chains;
using ControlCommandAnalyser.Parser.HelperParserParametr; // Для LoggerUtility
using Utilities;

namespace ControlCommandAnalyser.Parser.Kc
{
  /// <summary>
  /// Парсер для команд КС (контроль сопротивления).
  /// </summary>
  [AllowedKeys(AlgorithmKey.Б, AlgorithmKey.Д)]
  internal class KcCommandParser : ICommandParser
  {
    public bool CanParse(string mnemonic) => mnemonic == "КС";

    public BaseCommandModel Parse(string commandNumber, string mnemonic, int numberLine, List<string> lines)
    {
      LoggerUtility.LogInformation($"Начало парсинга команды: {commandNumber} {mnemonic}, строк: {lines?.Count ?? 0}");

      var model = new KsCommandModel
      {
        CommandNumber = commandNumber,
        SourceLines = new List<string>(lines),
        StartLineNumber = numberLine,
      };
      var rmCommandModel = CommandsModel.GetRMModel();

      if (rmCommandModel == null)
      {
        LoggerUtility.LogError($"Команда РМ не найдена");
        model.Errors.Add(KsErrors.EmptyPoints(numberLine, $"{commandNumber} {mnemonic}"));
      }

      if (lines == null || lines.Count == 0)
      {
        LoggerUtility.LogWarning($"Пустое тело команды: {commandNumber} {mnemonic} (строка {numberLine})");
        model.Errors.Add(KsErrors.EmptyCommandBody(numberLine, $"{commandNumber} {mnemonic}"));
        return model;
      }

      var errors = IndentationCheker.CheckIndentationErrors(lines, commandNumber, mnemonic);
      if (errors.Count > 0)
      {
        foreach (var error in errors)
        {
          LoggerUtility.LogError(error);
          model.Errors.Add(GeneralErrors.IndentationError(mnemonic, numberLine, $"{commandNumber} {mnemonic}"));
          return model;
        }
      }

      List<string> processedLines = CommentsParser.ParseComments(lines, model);
      // Убираем полностью пустые/пробельные строки (чтобы не таскать мусор)
      model.SourceLines = model.SourceLines
        .Where(l => !string.IsNullOrWhiteSpace(l))
        .ToList();

      // Склеиваем всё в одну строку и удаляем \r \n \t
      var body = string.Concat(processedLines.Count > 0 && processedLines.FindAll(l => string.IsNullOrEmpty(l) || string.IsNullOrWhiteSpace(l)).Count == 0 ?
        processedLines : model.SourceLines)
        .Replace("\r", "")
        .Replace("\n", "")
        .Replace("\t", "");

      // Для логов
      LoggerUtility.LogDebug($"Нормализованное тело команды (в одну строку): \"{body}\"");

      // Дальше работаем ТОЛЬКО с body:
      var remainder = body;

      var match = Regex.Match(remainder, @"^\s*\d+\s+[А-ЯA-Z]{2,}\s*(.*)$");
      if (match.Success)
        remainder = match.Groups[1].Value.Trim();

      string? lowerLimitResistance = null, higherLimitResistance = null, unit = null, time = null;

      var result = AlgorithmKeyParser.ExtractKeysWithTrailingCommaCheck(remainder);

      foreach (var (key, hasError) in result)
      {
        if (hasError)
        {
          LoggerUtility.LogWarning($"Пустое тело команды: {commandNumber} {mnemonic} (строка {numberLine})");
          model.Errors.Add(KsErrors.EmptyCommandBody(numberLine, $"{commandNumber} {mnemonic}"));
        }
        else
        {
          model.AlgorithmKey.Add(key);
          LoggerUtility.LogDebug($"Найден ключ алгоритма: {key}");
        }
      }

      // затем удаляем их из строки
      foreach (var key in model.AlgorithmKey)
      {
        remainder = Regex.Replace(
        remainder,
        $@"\b{Regex.Escape(key)}\s*,?",
        "",
        RegexOptions.IgnoreCase);
      }

      //TODO: проверить верхнюю и нижнюю границу сопротивления. Привести к системе СИ

      (lowerLimitResistance, higherLimitResistance, unit, remainder) = CommonParameterParser.ResistanceParser.ParseResistanceRange(remainder);
      LoggerUtility.LogDebug($"После парсинга напряжения: нижняя граница сопртивления='{lowerLimitResistance}',верхняя граница сопртивления='{higherLimitResistance}', единица измерения = '{unit}' remainder='{remainder}'");

      if (string.IsNullOrEmpty(lowerLimitResistance) && string.IsNullOrEmpty(higherLimitResistance))
      {
        model.Errors.Add(KsErrors.EmptyResistance(numberLine, $"{commandNumber} {mnemonic}"));
        LoggerUtility.LogWarning($"Не указано напряжение (строка {numberLine}): {commandNumber} {mnemonic}");
        if (!string.IsNullOrEmpty(remainder))
        {
          model.UnparsedParameters = "! Не распознанные параметры: ";
          model.UnparsedParameters += remainder;
          model.Errors.Add(GeneralErrors.UnrecognizedParameters(remainder, numberLine, $"{commandNumber} {mnemonic}"));
        }
        return model;
      }
      else
      {
        model.HigherLimitResistanceSource = higherLimitResistance;
        model.LowerLimitResistanceSource = lowerLimitResistance;
      }

      if (model.HigherLimitResistanceSource != null &&
        !string.IsNullOrEmpty(model.HigherLimitResistanceSource) && model.LowerLimitResistanceSource != null &&
        !string.IsNullOrEmpty(model.LowerLimitResistanceSource))
      {
        if (CommonParameterParser.ParseToDouble(model.LowerLimitResistanceSource) >= CommonParameterParser.ParseToDouble(model.HigherLimitResistanceSource))
        {
          LoggerUtility.LogWarning($"В команде {commandNumber} {mnemonic} (строка {numberLine}) нижняя граница сопротивления больше верхней границы сопротивления.");
          var description = "Нижняя граница сопротивления больше верхней границы сопротивления.";
          model.Errors.Add(KsErrors.ResistanceLimitsConflict(numberLine, $"{commandNumber} {mnemonic}", description));
          model.HigherLimitResistanceSource = null;
          model.LowerLimitResistanceSource = null;
        }
        else if (CommonParameterParser.ParseToDouble(model.HigherLimitResistanceSource) == CommonParameterParser.ParseToDouble(model.LowerLimitResistanceSource))
        {
          LoggerUtility.LogWarning($"В команде {commandNumber} {mnemonic} (строка {numberLine}) нижняя граница электрической емкости совпадает с верхней границей.");
          var description = "Нижняя граница сопротивления совпадает с верхней границей.";
          model.Errors.Add(KsErrors.ResistanceLimitsConflict(numberLine, $"{commandNumber} {mnemonic}", description));
        }
      }
      else
      {
        var meter = new DataBaseConfiguration.Services.Device.FastMeterServices().GetAll().FirstOrDefault();
        if (meter != null)
        {
          // TODO: добавить минимальное значение сопротивления для мультиметра

          var minResistance = 0.001 * 1_000;
          var maxResistance = meter.MaxContinuityResistance;
          if (!string.IsNullOrWhiteSpace(model.LowerLimitResistanceSource))
          {
            if (CommonParameterParser.ParseToDouble(model.LowerLimitResistanceSource) < minResistance)
            {
              LoggerUtility.LogWarning($"В команде {commandNumber} {mnemonic} (строка {numberLine}) нижняя граница электрической емкости меньше минимальной измеряемой емкости.");
              var description = "Нижняя граница электрической емкости меньше минимальной измеряемой емкости.";
              model.Errors.Add(IeErrors.CapacityLimitsConflict(numberLine, $"{commandNumber} {mnemonic}", description));
            }
            else
            {
              model.LowerLimitResistance = CommonParameterParser.ParseToDouble(model.LowerLimitResistanceSource);
              model.LowerLimitResistanceSource += " " + unit;
            }
          }
          else
          {
            model.LowerLimitResistance = minResistance;
            model.LowerLimitResistanceSource = $"{minResistance} Ом";
          }

          if (!string.IsNullOrWhiteSpace(model.HigherLimitResistanceSource))
          {
            if (CommonParameterParser.ParseToDouble(model.HigherLimitResistanceSource) > maxResistance)
            {
              LoggerUtility.LogWarning($"В команде {commandNumber} {mnemonic} (строка {numberLine}) верхняя граница электрической емкости больше максимально возможной емкости для измерения.");
              var description = "Верхняя граница электрической емкости больше максимально возможной емкости для измерения.";
              model.Errors.Add(IeErrors.CapacityLimitsConflict(numberLine, $"{commandNumber} {mnemonic}", description));
            }
            else
            {
              model.HigherLimitResistance = CommonParameterParser.ParseToDouble(model.HigherLimitResistanceSource);
              model.HigherLimitResistanceSource += " " + unit;
            }
          }
          else
          {
            model.HigherLimitResistance = maxResistance;
            model.HigherLimitResistanceSource = $"{maxResistance} Ом";
          }

          if (!string.IsNullOrEmpty(unit))
          {
            model.ResistanceUnit = unit;
          }
          else
          {
            model.ResistanceUnit = string.Empty;
          }
        }
      }

      if (HasInvalidParameterOrder(body, model.AlgorithmKey, lowerLimitResistance ?? higherLimitResistance, time, out string err))
      {
        model.Errors.Add(GeneralErrors.InvalidParameterOrder(mnemonic, numberLine, $"{commandNumber} {mnemonic}", err));
        LoggerUtility.LogWarning($"Ошибка порядка параметров (строка {numberLine}): {err}");
        return model;
      }

      string bodyNoWs = string.Concat(processedLines.Select(l => Regex.Replace(l ?? string.Empty, @"\s+", "")));

      // Ищем первую и последнюю '*'
      int firstStar = bodyNoWs.IndexOf('*');
      int lastStar = bodyNoWs.LastIndexOf('*');

      if (firstStar >= 0 && lastStar > firstStar)
      {
        // Выделяем блок точек (включительно) — PointParser сам Trim('*')
        string pointsBlob = bodyNoWs.Substring(firstStar, lastStar - firstStar + 1);
        model.PointsSourse = pointsBlob;
        LoggerUtility.LogDebug($"Парсинг точек из общего блока: '{pointsBlob}'");

        var (scheme, pointErrors) = PointParser.ParsePoints(pointsBlob, mnemonic, rmCommandModel);

        // Поднимем ошибки парсера точек
        if (pointErrors?.Count > 0)
        {
          foreach (var error in pointErrors)
          {
            error.SourceLineNumber = numberLine;
            error.Command = $"{commandNumber} {mnemonic}";
            model.Errors.Add(error);
            LoggerUtility.LogError(
              $"При парсинге точек команды {commandNumber} {mnemonic} произошла ошибка: {error.Description} (строка {error.SourceLineNumber}).");
          }
        }

        // Проверим, что схема непуста (есть хотя бы одна точка)
        if (scheme == null || scheme.IsEmpty())
        {
          LoggerUtility.LogWarning($"Не найдено ни одной точки (строка {numberLine}): {commandNumber} {mnemonic}");
          model.Errors.Add(IeErrors.EmptyPoints(numberLine, $"{commandNumber} {mnemonic}"));
        }
        else
        {
          model.Scheme = scheme; // ← просто присваиваем схему в модель
          LoggerUtility.LogInformation(
            $"Схема распознана: цепей={scheme.GroupModels?.Count ?? 0}, частей={scheme.CountParts()}, точек={scheme.CountPoints()}");
        }

        // Обновим remainder: оставим в нём только то, что до первой '*' в ПЕРВОЙ строке
        int idxStarInFirstLine = remainder.IndexOf('*');
        remainder = idxStarInFirstLine >= 0 ? remainder[..idxStarInFirstLine].Trim() : remainder.Trim();
      }
      else
      {
        // Во всём теле команды не нашли пары '*...*' → считаем, что точек нет
        LoggerUtility.LogWarning($"Во всём теле команды не найден блок точек '*...*' (строка {numberLine}): {commandNumber} {mnemonic}");
        model.Errors.Add(IeErrors.EmptyPoints(numberLine, $"{commandNumber} {mnemonic}"));
      }

      if (!string.IsNullOrEmpty(remainder))
      {
        model.UnparsedParameters = "! Не распознанные параметры: ";
        model.UnparsedParameters += remainder;
        model.Errors.Add(GeneralErrors.UnrecognizedParameters(remainder, numberLine, $"{commandNumber} {mnemonic}"));
      }

      // Валидация
      if (string.IsNullOrWhiteSpace(lowerLimitResistance) && string.IsNullOrWhiteSpace(higherLimitResistance) && string.IsNullOrWhiteSpace(time))
      {
        LoggerUtility.LogError($"Не удалось распознать параметры в строке: '{remainder}' (строка {numberLine})");
        model.Errors.Add(KsErrors.CannotParseParameters(remainder, numberLine, $"{commandNumber} {mnemonic}"));
      }

      AllowedKeysAttribute.ValidateKeysAndAttachErrors(model);

      LoggerUtility.LogInformation($"Завершён парсинг команды: {commandNumber} {mnemonic}");

      return model;
    }

    public static bool HasInvalidParameterOrder(string firstLine, List<string> algorithmKeys, string? resistanceStart, string? time, out string errorDescription)
    {
      errorDescription = string.Empty;

      int idxKey = -1;
      int idxTime = -1;
      int idxResistance = -1;
      int idxPoint = firstLine.IndexOf('*');

      // Позиция первого ключа
      foreach (var key in algorithmKeys)
      {
        int idx = firstLine.IndexOf(key, StringComparison.OrdinalIgnoreCase);
        if (idx >= 0 && (idxKey == -1 || idx < idxKey))
          idxKey = idx;
      }

      // Время
      if (!string.IsNullOrWhiteSpace(time))
      {
        idxTime = firstLine.IndexOf(time, StringComparison.OrdinalIgnoreCase);
      }

      // Сопротивление
      if (!string.IsNullOrWhiteSpace(resistanceStart))
      {
        idxResistance = firstLine.IndexOf(resistanceStart, StringComparison.OrdinalIgnoreCase);
      }

      // Проверка порядка
      // - Ключ должен идти до времени
      if (idxKey != -1 && idxTime != -1 && idxKey > idxTime)
      {
        errorDescription = "Ключ алгоритма указан после времени.";
        return true;
      }

      // - Ключ должен идти до сопротивления
      if (idxKey != -1 && idxResistance != -1 && idxKey > idxResistance)
      {
        errorDescription = "Ключ алгоритма указан после сопротивления.";
        return true;
      }

      // - Время должно быть после сопротивления
      if (idxTime != -1 && idxResistance != -1 && idxResistance > idxTime)
      {
        errorDescription = "Время указано до сопротивления.";
        return true;
      }

      // - Все параметры должны быть до точек
      if (idxPoint != -1)
      {
        if ((idxKey != -1 && idxKey > idxPoint)
         || (idxTime != -1 && idxTime > idxPoint)
         || (idxResistance != -1 && idxResistance > idxPoint))
        {
          errorDescription = "Один из параметров указан после точек.";
          return true;
        }
      }

      return false;
    }

  }
}
