using Errors.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Errors.Translation
{
  /// <summary>
  /// Содержит шаблоны ошибок, возникающих при парсинге выражений СИ-команд.
  /// </summary>
  public class SiErrors : IPointError
  {
    /// <summary>
    /// Ошибка: выражение не распознано.
    /// </summary>
    public static ErrorItem CannotParseExpression(string expr, int startLineNumber, string command) => new()
    {
      SourceLineNumber = startLineNumber,
      Command = command,
      Code = ErrorCode.Si_CannotParseExpression,
      Description = $"Не удалось распознать выражение: {expr}"
    };

    /// <summary>
    /// Ошибка: не удалось распознать параметры (напряжение, сопротивление, время).
    /// </summary>
    public static ErrorItem CannotParseParameters(string parameters, int startLineNumber, string command) => new()
    {
      SourceLineNumber = startLineNumber,
      Command = command,
      Code = ErrorCode.Si_CannotParseParameters,
      Description = $"Не удалось распознать параметры: {parameters}"
    };

    /// <summary>
    /// Ошибка: не указаны точки для измерения.
    /// </summary>
    public static ErrorItem EmptyPoints(int startLineNumber, string command) => new()
    {
      SourceLineNumber = startLineNumber,
      Command = command,
      Code = ErrorCode.Si_EmptyPoints,
      Description = "Не указаны точки для измерения."
    };

    /// <summary>
    /// Ошибка: не указано напряжение для измерения.
    /// </summary>
    public static ErrorItem EmptyVoltage(int startLineNumber, string command) => new()
    {
      SourceLineNumber = startLineNumber,
      Command = command,
      Code = ErrorCode.Si_EmptyVoltage,
      Description = "Не указано напряжение для измерения."
    };

    /// <summary>
    /// Ошибка: команда СИ не содержит ни одного параметра.
    /// </summary>
    public static ErrorItem EmptyCommandBody(int startLineNumber, string command) => new()
    {
      SourceLineNumber = startLineNumber,
      Command = command,
      Code = ErrorCode.Si_EmptyCommandBody,
      Description = "Команда СИ должна содержать хотя бы один параметр. Тело команды не может быть пустым."
    };

    /// <summary>
    /// Ошибка: Ошибка при проверке одно из разряда в групповом методе.
    /// </summary>
    /// <param name="command">Номер команды и мнемоника.</param>
    /// <param name="step">Номер разряда.</param>
    /// <param name="countStep">Кол-во разрядов.</param>
    /// <param name="resultMeasure">Результат измерения.</param>
    /// <returns></returns>
    public static ErrorItem WrongDigitCheckForGroupedMethod(string command, int step, int countStep, string resultMeasure) => new()
    {
      MeasureResult = resultMeasure,
      Command = command,
      Code = ErrorCode.Si_WrongDigitCheckForGroupedMethod,
      Description = $"Ошибка при проверке разряда {step} ({countStep}) при групповом методе."
    };

    /// <inheritdoc />
    public ErrorItem NodeExecutePointError(string command, List<string> point, string resultMeasure)
    {
      var error = new ErrorItem()
      {
        MeasureResult = resultMeasure,
        Command = command,
        Code = ErrorCode.Pr_NodeExecutePointError,
      };

      var str = string.Empty;
      foreach (var item in point)
      {
        str += $"#{item}";
      }

      error.Description = $"Ошибка при проверке цепи {str} при методе полного узла.";
      return error;
    }

    /// <inheritdoc />
    public ErrorItem ChainError(string command, string chain) => new()
    {
      Command = command,
      Code = ErrorCode.Si_ChainError,
      Description = $"Замкнутая цепь {chain}"
    };

    /// <inheritdoc />
    public ErrorItem DisconnectChainError(string command, string chain) => new()
    {
      Command = command,
      Code = ErrorCode.Si_ChainError,
      Description = $"Разрыв в цепи {chain}"
    };

    /// <inheritdoc />
    public ErrorItem PairError(string command, string pointFirst, string pointLast) => new()
    {
      Command = command,
      Code = ErrorCode.Si_PairError,
      Description = $"Замкнутая пара точек: {pointFirst}, {pointLast}"
    };

    /// <inheritdoc />
    public ErrorItem ChainPairError(string command, List<string> pointFirst, List<string> pointLast)
    {
      var eroror = new ErrorItem()
      {
        Command = command,
        Code = ErrorCode.Si_PairError,
      };

      var firstChain = string.Empty;
      foreach (var point in pointFirst)
      {
        firstChain += $"#{point.ToString()}";
      }

      var secondChain = string.Empty;
      foreach (var point in pointLast)
      {
        secondChain += $"#{point.ToString()}";
      }

      eroror.Description = $"Замкнутая пара цепей: {firstChain} и {secondChain}";
      return eroror;
    }

    /// <summary>
    /// Ошибка: конфликт границ сопротивления.
    /// </summary>
    public static ErrorItem ResistanceLimitsConflict(int startLineNumber, string command, string description) => new()
    {
      SourceLineNumber = startLineNumber,
      Command = command,
      Code = ErrorCode.Si_ResistanceLimitsConflict,
      Description = description
    };
  }
}
