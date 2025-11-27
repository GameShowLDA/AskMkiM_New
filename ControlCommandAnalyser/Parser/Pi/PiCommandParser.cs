using Errors.Translation;
using ControlCommandAnalyser.Model;
using ControlCommandAnalyser.Model.Chains;
using ControlCommandAnalyser.Parser.HelperParserParametr;
using ControlCommandAnalyser.Parser.Si; // Для LoggerUtility
using DTO.Device.Breakdown;
using DTO.Enum;
using System.Text.RegularExpressions;
using Utilities;
using Utilities.Extensions;

namespace ControlCommandAnalyser.Parser.Pi
{
  /// <summary>
  /// Парсер для команды ПИ (пробой изоляции).
  /// </summary>
  public class PiCommandParser : ICommandParser
  {
    public bool CanParse(MnemonicIdentifier mnemonic) => mnemonic.Mnemonic.MatchesEnum(Measurement.MeasurementTypeCommand.PI);

    public BaseCommandModel Parse(string commandNumber, string mnemonic, int numberLine, List<string> lines)
    {
      LoggerUtility.LogInformation($"Начало парсинга команды: {commandNumber} {mnemonic}, строк: {lines?.Count ?? 0}");

      var model = new PiCommandModel
      {
        CommandNumber = commandNumber,
        SourceLines = new List<string>(lines),
        StartLineNumber = numberLine,
      };
      var breakDown = AppConfiguration.ServiceLocator.GetRequired<IBreakdownTester>();
      if (breakDown == null)
      {
        LoggerUtility.LogError($"Не найдена пробойная установка.");
        model.Errors.Add(GeneralErrors.BreakDownNotFound(numberLine, $"{commandNumber} {mnemonic}"));
        return model;
      }
      else
      {
        var maxDCWVoltage = Measurement.MeasurementTypeCommand.PI_DCW.GetDisplayInfo().UpperLimit; // постоянный ток
        var minVoltage = Measurement.MeasurementTypeCommand.PI_ACW.GetDisplayInfo().LowerLimit;
        var maxACWVoltage = Measurement.MeasurementTypeCommand.PI_ACW.GetDisplayInfo().UpperLimit; // переменный ток

        var rmCommandModel = CommandsModel.GetRMModel();

        if (rmCommandModel == null)
        {
          LoggerUtility.LogError($"Команда РМ не найдена");
          model.Errors.Add(PiErrors.EmptyPoints(numberLine, $"{commandNumber} {mnemonic}"));
        }

        if (lines == null || lines.Count == 0)
        {
          LoggerUtility.LogWarning($"Пустое тело команды: {commandNumber} {mnemonic} (строка {numberLine})");
          model.Errors.Add(PiErrors.EmptyCommandBody(numberLine, $"{commandNumber} {mnemonic}"));
          return model;
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
          .Replace("\n", "");

        // Приведи ввод к «чистому» виду (NBSP, латиница->кириллица и т.п.)
        body = PiSiSplitter.PreNormalize(body);

        // Вырезаем номер + "ПИ" заранее, чтобы в сплит не летел заголовок
        var head = Regex.Match(body, @"^\s*\d+\s+ПИ\s*(.*)$", RegexOptions.IgnoreCase);
        var remainder = head.Success ? head.Groups[1].Value : body;

        LoggerUtility.LogDebug($"Хвост после ПИ: \"{remainder}\"");
        var (siPart, piPart, errs) = PiSiSplitter.SplitSiFromPiStrict(body);
        if (errs.Count > 0)
        {
          LoggerUtility.LogWarning($"Strict WS issues: {string.Join(" | ", errs)}");
        }

        var modelSi = new SiCommandModel();
        modelSi.SourceLines = new List<string> { siPart };
        var siRemainder = SiCommandParser.ManageSiParametersParse(modelSi, commandNumber, mnemonic, numberLine, siPart, breakDown);

        // Если СИ что-то не допарсила, логни отдельно (в модель СИ, не ПИ)
        if (!string.IsNullOrEmpty(siRemainder))
        {
          model.UnparsedParameters = "! Не распознанные параметры: ";
          model.UnparsedParameters += siRemainder;
          model.Errors.Add(GeneralErrors.UnrecognizedParameters(siRemainder, numberLine, $"{commandNumber} {mnemonic}"));
        }

        model.SiCommand = modelSi;
        if (modelSi.Errors.Count > 0)
        {
          model.Errors.AddRange(modelSi.Errors);
        }

        var remainderPi = piPart;

        // Если у тебя в ПИ могут ещё раз встречаться номер+мнемоника (на всякий), можно безопасно срезать:
        var match2 = Regex.Match(remainderPi, @"^\s*\d+\s+[А-ЯA-Z]{2,}\s*(.*)$");
        if (match2.Success) remainderPi = match2.Groups[1].Value.Trim();

        // --- парсим ПИ только из remainderPi ---
        string? voltage = null, time = null, unit = null, unitTime = null;

        var result = AlgorithmKeyParser.ExtractKeysWithTrailingCommaCheck(remainderPi, model);

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
          remainderPi = Regex.Replace(
          remainderPi,
          $@"\b{Regex.Escape(key)}\s*,?",
          "",
          RegexOptions.IgnoreCase);
        }

        // Парсим параметры
        (voltage, unit, remainderPi) = CommonParameterParser.VoltageParser.ParseVoltage(remainderPi);
        LoggerUtility.LogDebug($"После парсинга напряжения: voltage='{voltage}{unit}', remainder='{remainderPi}'");

        (time, unitTime, remainderPi) = CommonParameterParser.TimeParser.ParseTime(remainderPi);
        LoggerUtility.LogDebug($"После парсинга времени: time='{time}{unitTime}', remainder='{remainderPi}'");

        //if (remainderPi.Contains('+'))
        //{
        //  model.VoltageType = VoltageEnum.Type.DCW;
        //  remainderPi = remainderPi.Replace("+", string.Empty);
        //}

        bool isDcw = remainderPi.Contains('+');
        if (isDcw)
        {
          model.VoltageType = VoltageEnum.Type.DCW;
          remainderPi = remainderPi.Replace("+", string.Empty);
        }
        else
        {
          model.VoltageType = VoltageEnum.Type.ACW;
        }


        model.VoltageSource = voltage;

        if (model.VoltageSource != null)
        {
          model.Voltage = CommonParameterParser.ParseToDouble(model.VoltageSource);
          model.VoltageSource += unit;
          var maxVoltage = maxACWVoltage;
          var voltageType = string.Empty;
          if (model.VoltageType == VoltageEnum.Type.DCW)
          {
            maxVoltage = maxDCWVoltage;
          }
          voltageType = model.VoltageType == VoltageEnum.Type.DCW ? "постоянного" : "переменного";
          var voltageValue = UnitsConvertor.TryConvertBack(model.Voltage.Value, unit);
          if (model.Voltage.Value > maxVoltage)
          {
            var maxValue = UnitsConvertor.TryConvertBack(maxVoltage, "В");
            LoggerUtility.LogError($"В команде ПИ указано напряжение, превышающее максимально допустимое напряжение пробойной установки.");
            var description = $"В команде {commandNumber} {mnemonic} указано напряжение ({voltageValue.Item1} {voltageValue.Item2}), " +
              $"превышающий максимально допустимое напряжение пробойной установки ({maxValue.Item1} {maxValue.Item2}  " +
              $"для {voltageType} тока).";
            model.Errors.Add(GeneralErrors.VoltageConflict(numberLine, $"{commandNumber} {mnemonic}", description));
          }
          else if (model.Voltage.Value < minVoltage)
          {
            var minValue = UnitsConvertor.TryConvertBack(minVoltage, "В");
            LoggerUtility.LogError($"В команде ПИ указано напряжение, меньше минимально допустимого напряжения пробойной установки.");
            var description = $"В команде {commandNumber} {mnemonic} указано напряжение ({voltageValue.Item1} {voltageValue.Item2}), " +
              $"меньше минимально допустимого напряжения пробойной установки ({minValue.Item1} {minValue.Item2}" +
              $"для {voltageType} тока).";
            model.Errors.Add(GeneralErrors.VoltageConflict(numberLine, $"{commandNumber} {mnemonic}", description));
          }
          else
          {
            model.Voltage = model.Voltage.Value;
            model.VoltageSource = model.Voltage.Value.ToString() + unit;
          }
        }
        else
        {
          model.Voltage = minVoltage;
          model.VoltageSource = model.Voltage.Value.ToString() + "В";
          LoggerUtility.LogDebug($"В команде ПИ не указано напряжение. Установлено значение по умолчанию {minVoltage} В.");
          //model.Errors.Add(PiErrors.EmptyVoltage(numberLine, $"{commandNumber} {mnemonic}"));
        }

        model.Time = string.IsNullOrEmpty(time) || time == null ? 1 : CommonParameterParser.ParseToDouble(time);
        model.TimeSource = string.IsNullOrEmpty(time) || time == null ? "1c" : time + unitTime;

        if (model.Voltage == null)
        {
          model.Errors.Add(PiErrors.CannotParseParameters("Не указано напряжение", numberLine, $"{commandNumber} {mnemonic}"));
          LoggerUtility.LogWarning($"Не указано напряжение (строка {numberLine}): {commandNumber} {mnemonic}");
        }

        if (model.Time == null)
        {
          model.Errors.Add(PiErrors.CannotParseParameters("Не указано время", numberLine, $"{commandNumber} {mnemonic}"));
          LoggerUtility.LogWarning($"Не указано время (строка {numberLine}): {commandNumber} {mnemonic}");
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
          int idxStarInFirstLine = remainderPi.IndexOf('*');
          remainderPi = idxStarInFirstLine >= 0 ? remainderPi[..idxStarInFirstLine].Trim() : remainderPi.Trim();
          if (model.SiCommand.AlgorithmKey.Contains(TranslationKey.AlgorithmKey.П.ToString())
            || model.AlgorithmKey.Contains(TranslationKey.AlgorithmKey.П.ToString()))
          {
            // находим цепи точек из предыдущей команды проверки
            model.Scheme = CommandsModel.CheckKeyP(model, model.Scheme, model.SiCommand);
            model.SiCommand.Scheme = model.Scheme;
          }
          else if (model.SiCommand.AlgorithmKey.Contains(TranslationKey.AlgorithmKey.С.ToString())
            || model.AlgorithmKey.Contains(TranslationKey.AlgorithmKey.С.ToString()))
          {
            model.Scheme = CommandsModel.CheckKeyS(model.Scheme);
            model.SiCommand.Scheme = model.Scheme;
          }
        }
        else if (model.SiCommand.AlgorithmKey.Contains(TranslationKey.AlgorithmKey.П.ToString())
          || model.AlgorithmKey.Contains(TranslationKey.AlgorithmKey.П.ToString()))
        {
          // находим цепи точек из предыдущей команды проверки
          model.Scheme = CommandsModel.CheckKeyP(model.SiCommand, model.Scheme);
          model.SiCommand.Scheme = model.Scheme;
        }
        else if (model.SiCommand.AlgorithmKey.Contains(TranslationKey.AlgorithmKey.С.ToString())
          || model.AlgorithmKey.Contains(TranslationKey.AlgorithmKey.С.ToString()))
        {
          model.Scheme = CommandsModel.CheckKeyS(model.Scheme);
          model.SiCommand.Scheme = model.Scheme;
        }
        else
        {
          // Во всём теле команды не нашли пары '*...*' → считаем, что точек нет
          LoggerUtility.LogWarning($"Во всём теле команды не найден блок точек '*...*' (строка {numberLine}): {commandNumber} {mnemonic}");
          model.Errors.Add(IeErrors.EmptyPoints(numberLine, $"{commandNumber} {mnemonic}"));
        }

        CheckUnparsedParameters(commandNumber, mnemonic, numberLine, model, remainderPi);
        if (model.AlgorithmKey.Count == 0
          && model.SiCommand.AlgorithmKey.Count != 0
          && model.AlgorithmKey != null
          && model.SiCommand.AlgorithmKey != null)
        {
          model.AlgorithmKey = model.SiCommand.AlgorithmKey;
        }

        LoggerUtility.LogInformation($"Завершён парсинг команды: {commandNumber} {mnemonic}");

        model.SiCommand.CommandNumber = model.CommandNumber;
        model.SiCommand.FormattedStartLineNumber = model.FormattedStartLineNumber;
        model.SiCommand.Scheme = model.Scheme;
        model.SiCommand.StartLineNumber = model.StartLineNumber;

        return model;
      }
    }

    private static void CheckUnparsedParameters(string commandNumber, string mnemonic, int numberLine, PiCommandModel model, string remainderPi)
    {
      if (!string.IsNullOrEmpty(remainderPi))
      {
        model.UnparsedParameters = "! Не распознанные параметры: ";
        model.UnparsedParameters += remainderPi;
        model.Errors.Add(GeneralErrors.UnrecognizedParameters(remainderPi, numberLine, $"{commandNumber} {mnemonic}"));
      }
    }
  }
}
