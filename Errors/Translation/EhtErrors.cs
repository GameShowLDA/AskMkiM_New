using Errors.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Errors.Translation
{
  public class EhtErrors
  {
    /// <summary>
    /// Ошибка: не удалось распознать параметры (напряжение, сопротивление, время).
    /// </summary>
    public static ErrorItem CannotParseParameters(string parameters, int startLineNumber, string command) => new()
    {
      SourceLineNumber = startLineNumber,
      Command = command,
      Code = ErrorCode.Eht_CannotParseParameters,
      Description = $"Не удалось распознать параметры: {parameters}"
    };

    /// <summary>
    /// Ошибка: команда ПР не содержит ни одного параметра.
    /// </summary>
    public static ErrorItem EmptyCommandBody(int startLineNumber, string command) => new()
    {
      SourceLineNumber = startLineNumber,
      Command = command,
      Code = ErrorCode.Eht_EmptyCommandBody,
      Description = "Команда ЭТ должна содержать хотя бы один параметр. Тело команды не может быть пустым."
    };

    /// <summary>
    /// Ошибка: не указаны точки для измерения.
    /// </summary>
    public static ErrorItem EmptyPoints(int startLineNumber, string command) => new()
    {
      SourceLineNumber = startLineNumber,
      Command = command,
      Code = ErrorCode.Eht_EmptyPoints,
      Description = "Не указаны точки для измерения."
    };

    /// <summary>
    /// Ошибка: нижняя граница сопротивления больше верхней границы сопротивления.
    /// </summary>
    public static ErrorItem ResistanceLimitsConflict(int startLineNumber, string command, string description) => new()
    {
      SourceLineNumber = startLineNumber,
      Command = command,
      Code = ErrorCode.Eht_ResistanceLimitsConflict,
      Description = description
    };

    /// <summary>
    /// Ошибка: верхняя граница сопротивления больше максимально допустимой границы сопротивления.
    /// </summary>
    public static ErrorItem ResistanceMaxLimitsConflict(int startLineNumber, string command, double? maxResistance, string unit) => new()
    {
      SourceLineNumber = startLineNumber,
      Command = command,
      Code = ErrorCode.Eht_ResistanceMaxLimitsConflict,
      Description = $"Верхняя граница сопротивления больше максимально допустимой границы сопротивления({maxResistance} {unit})."
    };

    /// <summary>
    /// Ошибка: сопротивление между точками вне допустимого диапазона.
    /// </summary>
    public static ErrorItem ResistanceOutOfRange(string command, double measured, string firstPoint, string secondPoint, double lowerBound, double upperBound) => new()
    {
      Command = command,
      MeasureResult = measured.ToString() + " Ом",
      Code = ErrorCode.Eht_ResistanceOutOfRange,
      Description = $"{firstPoint}, {secondPoint} ({lowerBound:F3}–{upperBound:F3} Ом)"
    };

  }
}
