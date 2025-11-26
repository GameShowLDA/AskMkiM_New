using Errors.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Errors.Translation
{
  public class PrErrors : IPointError
  {
    /// <inheritdoc />

    public ErrorItem PairError(string command, string pointFirst, string pointLast) => new()
    {
      Command = command,
      Code = ErrorCode.Pr_PairError,
      Description = $"Замкнутая пара точек: {pointFirst}, {pointLast}"
    };

    /// <inheritdoc />
    public ErrorItem ChainPairError(string command, List<string> pointFirst, List<string> pointLast, string value)
    {
      var eroror = new ErrorItem()
      {
        Command = command,
        Code = ErrorCode.Pr_PairError,
        MeasureResult = value,
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

    /// <inheritdoc />
    public ErrorItem DisconnectChainError(string command, string chain, string measureResult) => new()
    {
      Command = command,
      Code = ErrorCode.Pr_ChainError,
      Description = $"Разрыв в цепи {chain}",
      MeasureResult = measureResult
    };

    /// <inheritdoc />
    public ErrorItem ChainError(string command, string chain) => new()
    {
      Command = command,
      Code = ErrorCode.Pr_ChainError,
      Description = $"Замкнутая цепь {chain}"
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

    /// <summary>
    /// Ошибка: команда ПР не содержит ни одного параметра.
    /// </summary>
    public static ErrorItem EmptyCommandBody(int startLineNumber, string command) => new()
    {
      SourceLineNumber = startLineNumber,
      Command = command,
      Code = ErrorCode.Pr_EmptyCommandBody,
      Description = "Команда ПР должна содержать хотя бы один параметр. Тело команды не может быть пустым."
    };

    /// <summary>
    /// Ошибка: не указаны точки для измерения.
    /// </summary>
    public static ErrorItem EmptyPoints(int startLineNumber, string command) => new()
    {
      SourceLineNumber = startLineNumber,
      Command = command,
      Code = ErrorCode.Pr_EmptyPoints,
      Description = "Не указаны точки для измерения."
    };

    /// <summary>
    /// Ошибка: не указано сопротивление.
    /// </summary>
    public static ErrorItem EmptyResistance(int startLineNumber, string command) => new()
    {
      SourceLineNumber = startLineNumber,
      Command = command,
      Code = ErrorCode.Pr_EmptyResistance,
      Description = "Не указано сопротивление."
    };


    /// <summary>
    /// Ошибка: нижняя граница сопротивления больше верхней границы сопротивления.
    /// </summary>
    public static ErrorItem ResistanceLimitsConflict(int startLineNumber, string command, string description) => new()
    {
      SourceLineNumber = startLineNumber,
      Command = command,
      Code = ErrorCode.Pr_ResistanceLimitsConflict,
      Description = description
    };


    /// <summary>
    /// Ошибка: верхняя граница сопротивления больше максимально допустимой границы сопротивления.
    /// </summary>
    public static ErrorItem ResistanceMaxLimitsConflict(int startLineNumber, string command, double? maxResistance, string unit) => new()
    {
      SourceLineNumber = startLineNumber,
      Command = command,
      Code = ErrorCode.Pr_ResistanceMaxLimitsConflict,
      Description = $"Верхняя граница сопротивления больше максимально допустимой границы сопротивления({maxResistance} {unit})."
    };

    /// <summary>
    /// Ошибка: не удалось распознать параметры (напряжение, сопротивление, время).
    /// </summary>
    public static ErrorItem CannotParseParameters(string parameters, int startLineNumber, string command) => new()
    {
      SourceLineNumber = startLineNumber,
      Command = command,
      Code = ErrorCode.Pr_CannotParseParameters,
      Description = $"Не удалось распознать параметры: {parameters}"
    };
  }
}
