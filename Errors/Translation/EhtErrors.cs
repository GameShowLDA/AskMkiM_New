using DTO.Base.Models;
using Errors.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Errors.Translation
{
  public class EhtErrors
  {
    /// <summary>
    /// Ошибка: не удалось распознать параметры (напряжение, сопротивление, время).
    /// </summary>
    public static ErrorItem CannotParseParameters(string parameters, int startLineNumber, string command,
      [CallerMemberName] string callerName = "",
      [CallerFilePath] string callerFile = "",
      [CallerLineNumber] int callerLine = 0) => new()
      {
        SourceLineNumber = startLineNumber,
        Command = command,
        Code = ErrorCode.Eht_CannotParseParameters,
        DebugInfo = $"{Path.GetFileName(callerFile)} → {callerName} (строка {callerLine})",
        Description = $"Не удалось распознать параметры: {parameters}"
      };

    /// <summary>
    /// Ошибка: команда ПР не содержит ни одного параметра.
    /// </summary>
    public static ErrorItem EmptyCommandBody(int startLineNumber, string command,
      [CallerMemberName] string callerName = "",
      [CallerFilePath] string callerFile = "",
      [CallerLineNumber] int callerLine = 0) => new()
      {
        SourceLineNumber = startLineNumber,
        Command = command,
        Code = ErrorCode.Eht_EmptyCommandBody,
        DebugInfo = $"{Path.GetFileName(callerFile)} → {callerName} (строка {callerLine})",
        Description = "Команда ЭТ должна содержать хотя бы один параметр. Тело команды не может быть пустым."
      };

    /// <summary>
    /// Ошибка: не указаны точки для измерения.
    /// </summary>
    public static ErrorItem EmptyPoints(int startLineNumber, string command,
      [CallerMemberName] string callerName = "",
      [CallerFilePath] string callerFile = "",
      [CallerLineNumber] int callerLine = 0) => new()
      {
        SourceLineNumber = startLineNumber,
        Command = command,
        Code = ErrorCode.Eht_EmptyPoints,
        DebugInfo = $"{Path.GetFileName(callerFile)} → {callerName} (строка {callerLine})",
        Description = "Не указаны точки для измерения."
      };

    /// <summary>
    /// Ошибка: нижняя граница сопротивления больше верхней границы сопротивления.
    /// </summary>
    public static ErrorItem ResistanceLimitsConflict(int startLineNumber, string command, string description,
      [CallerMemberName] string callerName = "",
      [CallerFilePath] string callerFile = "",
      [CallerLineNumber] int callerLine = 0) => new()
      {
        SourceLineNumber = startLineNumber,
        Command = command,
        Code = ErrorCode.Eht_ResistanceLimitsConflict,
        DebugInfo = $"{Path.GetFileName(callerFile)} → {callerName} (строка {callerLine})",
        Description = description
      };

    /// <summary>
    /// Ошибка: верхняя граница сопротивления больше максимально допустимой границы сопротивления.
    /// </summary>
    public static ErrorItem ResistanceMaxLimitsConflict(int startLineNumber, string command, double? maxResistance, string unit,
      [CallerMemberName] string callerName = "",
      [CallerFilePath] string callerFile = "",
      [CallerLineNumber] int callerLine = 0) => new()
      {
        SourceLineNumber = startLineNumber,
        Command = command,
        Code = ErrorCode.Eht_ResistanceMaxLimitsConflict,
        DebugInfo = $"{Path.GetFileName(callerFile)} → {callerName} (строка {callerLine})",
        Description = $"Верхняя граница сопротивления больше максимально допустимой границы сопротивления({maxResistance} {unit})."
      };

    /// <summary>
    /// Ошибка: сопротивление между точками вне допустимого диапазона.
    /// </summary>
    public static ErrorItem ResistanceOutOfRange(string command, double measured, string firstPoint, string secondPoint, double lowerBound, double upperBound,
      [CallerMemberName] string callerName = "",
      [CallerFilePath] string callerFile = "",
      [CallerLineNumber] int callerLine = 0) => new()
      {
        Command = command,
        MeasureResult = measured.ToString() + " Ом",
        Code = ErrorCode.Eht_ResistanceOutOfRange,
        DebugInfo = $"{Path.GetFileName(callerFile)} → {callerName} (строка {callerLine})",
        Description = $"{firstPoint}, {secondPoint} ({lowerBound:F3}–{upperBound:F3} Ом)"
      };

    /// <summary>
    /// Ошибка: указанная точка не подключена.
    /// </summary>
    public static ErrorItem PointNotConnected(string command, string point,
      [CallerMemberName] string callerName = "",
      [CallerFilePath] string callerFile = "",
      [CallerLineNumber] int callerLine = 0) => new()
      {
        Command = command,
        Code = ErrorCode.Eht_PointNotConnected,
        DebugInfo = $"{Path.GetFileName(callerFile)} → {callerName} (строка {callerLine})",
        Description = $"Точка '{point}' не подключена."
      };

    /// <summary>
    /// Ошибка: между указанными точками разрыв цепи (Overload).
    /// </summary>
    public static ErrorItem CircuitOverload(string command, string firstPoint, string secondPoint,
      [CallerMemberName] string callerName = "",
      [CallerFilePath] string callerFile = "",
      [CallerLineNumber] int callerLine = 0) => new()
      {
        Command = command,
        MeasureResult = "Overload",
        Code = ErrorCode.Eht_CircuitOverload,
        DebugInfo = $"{Path.GetFileName(callerFile)} → {callerName} (строка {callerLine})",
        Description = $"*{firstPoint}**{secondPoint}*"
      };
  }
}
