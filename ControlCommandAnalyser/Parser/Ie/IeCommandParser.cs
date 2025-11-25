using Errors.Translation;
using ControlCommandAnalyser.Model;
using ControlCommandAnalyser.Model.Chains;
using ControlCommandAnalyser.Parser.HelperParserParametr;
using DTO.Enum;
using System.Text.RegularExpressions;
using System.Windows;
using Utilities;

namespace ControlCommandAnalyser.Parser.Ie
{
  /// <summary>
  /// Парсер для команд ИЕ (измерение емкости).
  /// </summary>
  [AllowedKeys(AlgorithmKey.Д)]
  internal class IeCommandParser : ICommandParser
  {
    public bool CanParse(MnemonicIdentifier mnemonic)
    => mnemonic.Mnemonic.MatchesEnum(Measurement.MeasurementTypeCommand.IE);

    public BaseCommandModel Parse(string commandNumber, string mnemonic, int numberLine, List<string> lines)
    {
      LoggerUtility.LogInformation($"Начало парсинга команды: {commandNumber} {mnemonic}, строк: {lines?.Count ?? 0}");
      var model = new IeCommandModel
      {
        CommandNumber = commandNumber,
        SourceLines = new List<string>(lines),
        StartLineNumber = numberLine,
      };

      var rmCommandModel = CommandsModel.GetRMModel();

      if (rmCommandModel == null)
      {
        LoggerUtility.LogError($"Команда РМ не найдена");
        model.Errors.Add(IeErrors.EmptyPoints(numberLine, $"{commandNumber} {mnemonic}"));
      }

      if (lines == null || lines.Count == 0)
      {
        LoggerUtility.LogWarning($"Пустое тело команды: {commandNumber} {mnemonic} (строка {numberLine})");
        model.Errors.Add(IeErrors.EmptyCommandBody(numberLine, $"{commandNumber} {mnemonic}"));
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

      string? lowerLimitCapacity = null, higherLimitCapacity = null, unit = null;

      var result = AlgorithmKeyParser.ExtractKeysWithTrailingCommaCheck(remainder, model);

      foreach (var (key, hasError) in result)
      {
        if (hasError)
        {
          model.Errors.Add(GeneralErrors.WrongKey(numberLine, mnemonic, $"{commandNumber} {mnemonic}", key));
        }
        else
        {
          model.AlgorithmKey.Add(key);
          LoggerUtility.LogDebug($"Найден ключ алгоритма: {key}");
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

      (lowerLimitCapacity, higherLimitCapacity, unit, remainder) = CommonParameterParser.CapacityParser.ParseCapacityRange(remainder);
      LoggerUtility.LogDebug($"После парсинга электрической ёмкости: нижняя='{lowerLimitCapacity}', верхняя='{higherLimitCapacity}', единица='{unit}', remainder='{remainder}'");

      // 1️⃣ Если нижняя граница вообще не распознана — это всегда ошибка
      if (string.IsNullOrEmpty(lowerLimitCapacity))
      {
        model.Errors.Add(IeErrors.EmptyLowerCapacity(numberLine, $"{commandNumber} {mnemonic}"));
        LoggerUtility.LogWarning($"Не указана нижняя граница электрической емкости (строка {numberLine}): {commandNumber} {mnemonic}");

        if (!string.IsNullOrEmpty(remainder))
        {
          model.UnparsedParameters = "! Не распознанные параметры: " + remainder;
          model.Errors.Add(GeneralErrors.UnrecognizedParameters(remainder, numberLine, $"{commandNumber} {mnemonic}"));
        }

        return model;
      }

      // 3️⃣ Парсим численные значения
      double? lower = CommonParameterParser.ParseToDouble(lowerLimitCapacity);
      double? higher = !string.IsNullOrWhiteSpace(higherLimitCapacity) ? CommonParameterParser.ParseToDouble(higherLimitCapacity) : null;

      // 4️⃣ Задаём диапазон допустимых значений
      var meter = new DataBaseConfiguration.Services.Device.FastMeterServices().GetAll().FirstOrDefault();
      if (meter == null)
      {
        LoggerUtility.LogError($"Не найден быстрый измеритель.");
        model.Errors.Add(GeneralErrors.FastMeterNotFound(numberLine, $"{commandNumber} {mnemonic}"));
        return model;
      }
      else
      {
        var commandInfo = EnumExtensions.GetDisplayInfo(Measurement.MeasurementTypeCommand.IE);

        double minCapacity = commandInfo.LowerLimit;
        double maxCapacity = commandInfo.UpperLimit;
        string defaultCapacityUnit = commandInfo.Unit;

        var lowerLimit = UnitsConvertor.TryParseValue($"{minCapacity}", commandInfo.Unit);
        var higherLimit = UnitsConvertor.TryParseValue($"{maxCapacity}", commandInfo.Unit);

        // 5️⃣ Флаг ошибок
        bool hasErrors = false;

        // 6️⃣ Проверка: если обе границы заданы
        if (lower.HasValue && higher.HasValue)
        {
          if (lower.Value >= higher.Value)
          {
            var lowerValue = UnitsConvertor.TryConvertBack(lower.Value, unit);
            var higherValue = UnitsConvertor.TryConvertBack(higher.Value, unit);
            LoggerUtility.LogWarning($"В команде {commandNumber} {mnemonic} (строка {numberLine}) нижняя граница электрической емкости больше или равна верхней.");
            model.Errors.Add(IeErrors.CapacityLimitsConflict(numberLine, $"{commandNumber} {mnemonic}",
              $"Нижняя граница электрической емкости ({lowerValue.Item1} {lowerValue.Item2}) " +
              $"больше или равна верхней ({higherValue.Item1} {higherValue.Item2})."));
            hasErrors = true;
          }
        }

        // 7️⃣ Проверка нижней границы
        if (lower.HasValue && !hasErrors)
        {
          var lowerValue = UnitsConvertor.TryParseValue($"{lower.Value}", unit);
          if (lowerValue < lowerLimit)
          {
            LoggerUtility.LogWarning($"В команде {commandNumber} {mnemonic} (строка {numberLine}) " +
              $"нижняя граница электрической емкости меньше минимально измеряемой ({lowerLimit} {defaultCapacityUnit}).");
            model.Errors.Add(IeErrors.CapacityLimitsConflict(numberLine, $"{commandNumber} {mnemonic}",
              $"Нижняя граница электрической емкости ({lowerValue} {unit}) " +
              $"меньше минимально измеряемой ({lowerLimit} {defaultCapacityUnit})."));
            hasErrors = true;
          }
          if (lowerValue > higherLimit)
          {
            LoggerUtility.LogWarning($"В команде {commandNumber} {mnemonic} (строка {numberLine}) верхняя граница электрической емкости больше максимально возможной ({maxCapacity}).");
            model.Errors.Add(IeErrors.CapacityLimitsConflict(numberLine, $"{commandNumber} {mnemonic}",
              $"Нижняя граница электрической емкости ({lowerValue} {unit}) больше максимально возможной ({higherLimit} {defaultCapacityUnit})."));
            hasErrors = true;
          }
        }

        // 8️⃣ Проверка верхней границы (если она есть)
        if (higher.HasValue && !hasErrors)
        {
          var higherValue = UnitsConvertor.TryParseValue($"{higher.Value}", unit);

          if (higherValue > higherLimit)
          {
            LoggerUtility.LogWarning($"В команде {commandNumber} {mnemonic} (строка {numberLine}) " +
              $"верхняя граница электрической емкости больше максимально возможной ({maxCapacity}).");
            model.Errors.Add(IeErrors.CapacityLimitsConflict(numberLine, $"{commandNumber} {mnemonic}",
              $"Верхняя граница электрической емкости ({higherValue} {unit}) " +
              $"больше максимально возможной ({higherLimit} {defaultCapacityUnit})."));
            hasErrors = true;
          }
          if (higherValue < lowerLimit)
          {
            LoggerUtility.LogWarning($"В команде {commandNumber} {mnemonic} (строка {numberLine}) " +
              $"нижняя граница электрической емкости меньше минимально измеряемой ({lowerLimit} {defaultCapacityUnit}).");
            model.Errors.Add(IeErrors.CapacityLimitsConflict(numberLine, $"{commandNumber} {mnemonic}",
              $"Верхняя граница электрической емкости ({higherValue} {unit}) " +
              $"меньше минимально измеряемой ({lowerLimit} {defaultCapacityUnit})."));
            hasErrors = true;
          }
        }

        // 9️⃣ Установка значений (только если всё прошло проверки)
        if (hasErrors == false)
        {
          // нижняя всегда должна быть → если есть — устанавливаем
          model.LowerLimitCapacity = lower.Value;
          model.LowerLimitCapacitySource = $"{lower.Value} {unit}";

          // верхняя: если есть — используем, если нет — ставим дефолт
          double finalHigher = higher ?? maxCapacity;
          model.HigherLimitCapacity = finalHigher;
          model.HigherLimitCapacitySource = $"{finalHigher} {unit}";

          model.CapacityUnit = unit ?? string.Empty;
        }

        if (HasInvalidParameterOrder(body, model.AlgorithmKey, lowerLimitCapacity ?? higherLimitCapacity, out string err))
        {
          model.Errors.Add(GeneralErrors.InvalidParameterOrder(mnemonic, numberLine, $"{commandNumber} {mnemonic}", err));
          LoggerUtility.LogWarning($"Ошибка порядка параметров (строка {numberLine}): {err}");
          return model;
        }

        //var schemeModel = new SchemeModel(new List<ChainModel>());
        // --- новый разбор блока точек между первой и последней '*' во всём теле команды ---
        // Собираем всё тело команды (включая последующие строки), убираем все пробельные символы
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
        if (string.IsNullOrWhiteSpace(lowerLimitCapacity) && string.IsNullOrWhiteSpace(higherLimitCapacity))
        {
          LoggerUtility.LogError($"Не удалось распознать параметры в строке: '{remainder}' (строка {numberLine})");
          model.Errors.Add(IeErrors.CannotParseParameters(remainder, numberLine, $"{commandNumber} {mnemonic}"));
        }

        AllowedKeysAttribute.ValidateKeysAndAttachErrors(model);

        LoggerUtility.LogInformation($"Завершён парсинг команды: {commandNumber} {mnemonic}");

        return model;
      }
    }

    public static bool HasInvalidParameterOrder(string firstLine, List<string> algorithmKeys, string? resistanceStart, out string errorDescription)
    {
      errorDescription = string.Empty;

      int idxKey = -1;
      int idxResistance = -1;
      int idxPoint = firstLine.IndexOf('*');

      // Позиция первого ключа
      foreach (var key in algorithmKeys)
      {
        int idx = firstLine.IndexOf(key, StringComparison.OrdinalIgnoreCase);
        if (idx >= 0 && (idxKey == -1 || idx < idxKey))
          idxKey = idx;
      }

      // Сопротивление
      if (!string.IsNullOrWhiteSpace(resistanceStart))
      {
        idxResistance = firstLine.IndexOf(resistanceStart, StringComparison.OrdinalIgnoreCase);
      }

      // - Ключ должен идти до сопротивления
      if (idxKey != -1 && idxResistance != -1 && idxKey > idxResistance)
      {
        errorDescription = "Ключ алгоритма указан после электрической емкости.";
        return true;
      }

      // - Все параметры должны быть до точек
      if (idxPoint != -1)
      {
        if ((idxKey != -1 && idxKey > idxPoint)
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
