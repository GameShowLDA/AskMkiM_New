using Errors.Translation;
using ControlCommandAnalyser.Model;
using ControlCommandAnalyser.Model.Chains;
using ControlCommandAnalyser.Parser.HelperParserParametr;
using DTO.Device.Breakdown;
using DTO.Enum;
using System.Text.RegularExpressions;
using Utilities;
using ControlCommandAnalyser.Attributes;

namespace ControlCommandAnalyser.Parser.Si
{
  /// <summary>
  /// Парсер для команд СИ (сопротивление изоляции).
  /// </summary>
  public class SiCommandParser : ICommandParser
  {
    public bool CanParse(MnemonicIdentifier mnemonic)
    => mnemonic.Mnemonic.MatchesEnum(Measurement.MeasurementTypeCommand.CI);

    public BaseCommandModel Parse(string commandNumber, string mnemonic, int numberLine, List<string> lines)
    {
      LoggerUtility.LogInformation($"Начало парсинга команды: {commandNumber} {mnemonic}, строк: {lines?.Count ?? 0}");

      var model = new SiCommandModel
      {
        CommandNumber = commandNumber,
        SourceLines = lines?.ToList() ?? new List<string>(),
        StartLineNumber = numberLine,
      };

      if (lines == null || lines.Count == 0)
      {
        LoggerUtility.LogWarning($"Пустое тело команды: {commandNumber} {mnemonic} (строка {numberLine})");
        model.Errors.Add(SiErrors.EmptyCommandBody(numberLine, $"{commandNumber} {mnemonic}"));
      }

      var rmCommandModel = CommandsModel.GetRMModel();

      if (rmCommandModel == null)
      {
        LoggerUtility.LogError($"Команда РМ не найдена");
        model.Errors.Add(SiErrors.EmptyPoints(numberLine, $"{commandNumber} {mnemonic}"));
      }

      var breakDown = AppConfiguration.ServiceLocator.GetRequired<IBreakdownTester>();
      if (breakDown == null)
      {
        LoggerUtility.LogError($"Не найден быстрый измеритель.");
        model.Errors.Add(GeneralErrors.FastMeterNotFound(numberLine, $"{commandNumber} {mnemonic}"));
        return model;
      }
      else
      {
        var commandInfo = EnumExtensions.GetDisplayInfo(Measurement.MeasurementTypeCommand.CI);

        var maxVoltage = breakDown.MaxVoltage;

        string body = AllLinesInOne(model, lines);

        // Дальше работаем ТОЛЬКО с body:
        var remainder = body;

        remainder = ManageSiParametersParse(model, commandNumber, mnemonic, numberLine, remainder, breakDown);

        string bodyNoWs = string.Concat(lines.Select(l => Regex.Replace(l ?? string.Empty, @"\s+", "")));

        // Ищем первую и последнюю '*'
        int firstStar = bodyNoWs.IndexOf('*');
        int lastStar = bodyNoWs.LastIndexOf('*');

        if (firstStar >= 0 && lastStar > firstStar)
        {
          remainder = ParsePoints(commandNumber, mnemonic, numberLine, model, rmCommandModel, remainder, bodyNoWs, firstStar, lastStar);
        }
        else if (model.AlgorithmKey.Contains(TranslationKey.AlgorithmKey.П.ToString()))
        {
          // находим цепи точек из предыдущей команды проверки
          model.Scheme = CommandsModel.CheckKeyP(model, model.Scheme);
        }
        else if (model.AlgorithmKey.Contains(TranslationKey.AlgorithmKey.С.ToString()))
        {
          model.Scheme = CommandsModel.CheckKeyS(model.Scheme);
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

        AllowedKeysAttribute.ValidateKeysAndAttachErrors(model);

        LoggerUtility.LogInformation($"Завершён парсинг команды: {commandNumber} {mnemonic}");

        return model;
      }
    }


    private static string ParsePoints(string commandNumber, string mnemonic, int numberLine, SiCommandModel model, RmCommandModel rmCommandModel, string remainder, string bodyNoWs, int firstStar, int lastStar)
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

      if (scheme == null || scheme.IsEmpty())
      {
        LoggerUtility.LogWarning($"Не найдено ни одной точки (строка {numberLine}): {commandNumber} {mnemonic}");
        model.Errors.Add(SiErrors.EmptyPoints(numberLine, $"{commandNumber} {mnemonic}"));
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
      if (model.AlgorithmKey.Contains(TranslationKey.AlgorithmKey.П.ToString()))
      {
        // находим цепи точек из предыдущей команды проверки
        model.Scheme = CommandsModel.CheckKeyP(model, model.Scheme);
      }
      else if (model.AlgorithmKey.Contains(TranslationKey.AlgorithmKey.С.ToString()))
      {
        model.Scheme = CommandsModel.CheckKeyS(model.Scheme);
      }
      return remainder;
    }

    public static string ManageSiParametersParse(SiCommandModel model, string commandNumber, string mnemonic, int numberLine, string remainder,
      IBreakdownTester breakDown)
    {
      var body = remainder;
      var match = Regex.Match(remainder, @"^\s*\d+\s+[А-ЯA-Z]{2,}\s*(.*)$");
      if (match.Success)
        remainder = match.Groups[1].Value.Trim();

      // сначала извлекаем ключи
      remainder = KeyParser.ParseKeys(numberLine, model, remainder);

      remainder = ExtractSiParameters(commandNumber, mnemonic, numberLine, model, remainder, breakDown);

      return remainder;
    }

    // TODO: добавить минимальное напряжение, максимальное для переменного тока и постоянного, минимальное и максимальное сопротивление для ППУ
    private static string ExtractSiParameters(string commandNumber, string mnemonic, int numberLine, SiCommandModel model,
      string remainder, IBreakdownTester breakDown)
    {
      var commandInfo = EnumExtensions.GetDisplayInfo(Measurement.MeasurementTypeCommand.CI);

      var minVoltage = breakDown.IRMinVoltage;
      var maxVoltage = breakDown.MaxVoltage;
      double minResistance = commandInfo.LowerLimit;
      double maxResistance = commandInfo.UpperLimit;
      string defaultResistainceunit = commandInfo.Unit;
      string voltage = string.Empty, resistance = string.Empty, time = string.Empty, unit = string.Empty, unitTime = string.Empty, unitResistance = string.Empty;

      (voltage, unit, remainder) = CommonParameterParser.VoltageParser.ParseVoltage(remainder);
      LoggerUtility.LogDebug($"После парсинга напряжения: voltage='{voltage}{unit}', remainder='{remainder}'");

      (resistance, unitResistance, remainder) = CommonParameterParser.ResistanceParser.ParseResistance(remainder);
      LoggerUtility.LogDebug($"После парсинга сопротивления: resistance='{resistance}{unitResistance}', remainder='{remainder}'");
      if (!string.IsNullOrEmpty(resistance))
      {
        resistance = UnitsConvertor.ConvertToMOhms(CommonParameterParser.ParseToDouble(resistance), unitResistance).ToString();
      }
      unitResistance = "МОм";

      (time, unitTime, remainder) = CommonParameterParser.TimeParser.ParseTime(remainder);
      LoggerUtility.LogDebug($"После парсинга времени: time='{time}{unitTime}', remainder='{remainder}'");

      if (voltage != null)
      {
        model.Voltage = CommonParameterParser.ParseToDouble(voltage);
        var voltageValue = UnitsConvertor.TryConvertBack(model.Voltage.Value, unit);

        if (model.Voltage.HasValue)
        {
          if (model.Voltage.Value > maxVoltage)
          {
            var maxValue = UnitsConvertor.TryConvertBack(maxVoltage, "В");

            LoggerUtility.LogError($"В команде {commandNumber} {mnemonic} указано напряжение ({voltageValue.Item1} {voltageValue.Item2}), " +
              $"превышающее максимально допустимое напряжение пробойной установки ({maxValue.Item1} {maxValue.Item2}).");
            var description = $"В команде {commandNumber} {mnemonic} указано напряжение ({voltageValue.Item1} {voltageValue.Item2}), " +
              $"превышающее максимально допустимое напряжение пробойной установки ({maxValue.Item1} {maxValue.Item2}).";
            model.Errors.Add(GeneralErrors.VoltageConflict(numberLine, $"{commandNumber} {mnemonic}", description));
          }
          else if (model.Voltage.Value < minVoltage)
          {
            var minValue = UnitsConvertor.TryConvertBack(minVoltage, "В");

            LoggerUtility.LogError($"В команде {commandNumber} {mnemonic} указано напряжение ({model.Voltage.Value}), " +
              $"меньше минимально допустимого напряжения пробойной установки ({minValue.Item1} {minValue.Item2}).");
            var description = $"В команде {commandNumber} {mnemonic} указано напряжение ({model.Voltage.Value}), " +
              $"меньше минимально допустимого напряжения пробойной установки ({minValue.Item1} {minValue.Item2}).";
            model.Errors.Add(GeneralErrors.VoltageConflict(numberLine, $"{commandNumber} {mnemonic}", description));
          }
          else
          {
            model.Voltage = model.Voltage.Value;
            model.VoltageSource = model.Voltage.Value.ToString() + unit;
          }
        }
      }
      else
      {
        LoggerUtility.LogError($"В команде СИ не указано напряжение.");
        model.Errors.Add(SiErrors.EmptyVoltage(numberLine, $"{commandNumber} {mnemonic}"));
      }

      double? resistanceValue;
      if (string.IsNullOrEmpty(resistance) || resistance == null)
      {
        resistance = "100";
        resistanceValue = 100;
        unitResistance = "МОм";
        LoggerUtility.LogDebug($"Для сопротивления установлено значение по умолчанию '100<МОм'");
        model.Warnings.Add(GeneralWarnings.DefaultResistainceLowLimit(model.StartLineNumber, $"{commandNumber} {mnemonic}", $"{resistance} {unitResistance}"));
      }
      else
      {
        resistanceValue = CommonParameterParser.ParseToDouble(resistance);
      }

      if (resistanceValue.HasValue)
      {
        //var maxValue = UnitsConvertor.TryParseValue($"{maxResistance}", defaultResistainceunit);
        //var minValue = UnitsConvertor.TryParseValue($"{minResistance}", defaultResistainceunit);
        //var resistanceFormatted = UnitsConvertor.TryConvertBack(resistanceValue.Value, unitResistance);
        if (resistanceValue.Value > maxResistance)
        {
          LoggerUtility.LogError($"В команде СИ указано сопротивление, превышающее максимально допустимое сопротивление пробойной установки.");
          var description = $"В команде {commandNumber} {mnemonic} указано сопротивление ({resistance} {unitResistance}), " +
            $"превышающий максимально допустимое сопротивление пробойной установки ({maxResistance} {defaultResistainceunit}).";
          model.Errors.Add(SiErrors.ResistanceLimitsConflict(numberLine, $"{commandNumber} {mnemonic}", description));
        }
        else if (resistanceValue.Value < minResistance)
        {
          LoggerUtility.LogError($"В команде СИ указано сопротивление, меньше минимально допустимого сопротивления пробойной установки.");
          var description = $"В команде {commandNumber} {mnemonic} указано сопротивление ({resistance} {unitResistance}), " +
            $"меньше минимально допустимого напряжения пробойной установки ({minResistance} {defaultResistainceunit}).";
          model.Errors.Add(SiErrors.ResistanceLimitsConflict(numberLine, $"{commandNumber} {mnemonic}", description));
        }
        else
        {
          model.Resistance = resistanceValue.Value;
        }
      }
      model.ResistanceSource = resistance + "<" + unitResistance;
      model.ResistanceUnit = unitResistance;

      double? timeValue;
      if (string.IsNullOrEmpty(time) || time == null)
      {
        LoggerUtility.LogDebug($"Для времени установлено значение по умолчанию 5 с.'");
        time = "5с";
        timeValue = 5;
        model.Warnings.Add(GeneralWarnings.DefaultTime(model.StartLineNumber, $"{commandNumber} {mnemonic}", time));
      }
      else
      {
        timeValue = CommonParameterParser.ParseToDouble(time);
      }
      if (timeValue.HasValue)
      {
        model.Time = timeValue.Value;
      }
      model.TimeSource = time + unitTime;

      ValidateSiParameters(commandNumber, mnemonic, numberLine, model, voltage, resistance, time);

      return remainder;
    }

    private static void ValidateSiParameters(string commandNumber, string mnemonic, int numberLine, SiCommandModel model, string? voltage, string? resistance, string time)
    {
      if (voltage != null && string.IsNullOrWhiteSpace(voltage))
      {
        model.Errors.Add(SiErrors.CannotParseParameters("Не указано напряжение", numberLine, $"{commandNumber} {mnemonic}"));
        LoggerUtility.LogWarning($"Не указано напряжение (строка {numberLine}): {commandNumber} {mnemonic}");
      }

      if (string.IsNullOrWhiteSpace(resistance))
      {
        model.Errors.Add(SiErrors.CannotParseParameters("Не указано сопротивление", numberLine, $"{commandNumber} {mnemonic}"));
        LoggerUtility.LogWarning($"Не указано сопротивление (строка {numberLine}): {commandNumber} {mnemonic}");
      }

      if (string.IsNullOrWhiteSpace(time))
      {
        model.Errors.Add(SiErrors.CannotParseParameters("Не указано время", numberLine, $"{commandNumber} {mnemonic}"));
        LoggerUtility.LogWarning($"Не указано время (строка {numberLine}): {commandNumber} {mnemonic}");
      }
    }

    private static string ExtractSiKeys(string commandNumber, string mnemonic, int numberLine, SiCommandModel model, string body, string remainder)
    {
      var result = AlgorithmKeyParser.ExtractKeysWithTrailingCommaCheck(body, model);

      foreach (var (key, hasError) in result)
      {
        if (hasError)
        {
          model.Errors.Add(GeneralErrors.WrongKey(numberLine, mnemonic, $"{commandNumber} {mnemonic}", key));
        }
        else
        {
          if (!model.AlgorithmKey.Contains(key))
          {
            model.AlgorithmKey.Add(key);
            LoggerUtility.LogDebug($"Найден ключ алгоритма: {key}");
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

    private static string AllLinesInOne(SiCommandModel model, List<string> lines)
    {
      List<string> processedLines = CommentsParser.ParseComments(lines, model);
      lines.Clear();
      lines.AddRange(processedLines);
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
      return body;
    }
  }
}