using Errors.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Errors.Translation
{
  /// <summary>
  /// Содержит шаблоны ошибок, возникающих при парсинге выражений КС-команд.
  /// </summary>
  public class KsErrors : IPointError
  {
    /// <summary>
    /// Ошибка: команда КС не содержит ни одной границы для сопротивления.
    /// </summary>
    public static ErrorItem EmptyResistance(int startLineNumber, string command) => new()
    {
      SourceLineNumber = startLineNumber,
      Command = command,
      Code = ErrorCode.Ks_EmptyResistance,
      Description = "Команда КС должна содержать хотя бы одну из границ сопротивления. Сопротивление не может быть не задано."
    };
    /// <summary>
    /// Ошибка: нижняя граница сопротивления больше верхней границы сопротивления.
    /// </summary>
    public static ErrorItem ResistanceLimitsConflict(int startLineNumber, string command, string description) => new()
    {
      SourceLineNumber = startLineNumber,
      Command = command,
      Code = ErrorCode.Ks_CapacityLimitsConflict,
      Description = description
    };

    /// <summary>
    /// Ошибка: не удалось распознать параметры (напряжение, сопротивление, время).
    /// </summary>
    public static ErrorItem CannotParseParameters(string parameters, int startLineNumber, string command) => new()
    {
      SourceLineNumber = startLineNumber,
      Command = command,
      Code = ErrorCode.Ks_CannotParseParameters,
      Description = $"Не удалось распознать параметры: {parameters}"
    };

    /// <summary>
    /// Ошибка: не указаны точки для измерения.
    /// </summary>
    public static ErrorItem EmptyPoints(int startLineNumber, string command) => new()
    {
      SourceLineNumber = startLineNumber,
      Command = command,
      Code = ErrorCode.Ks_EmptyPoints,
      Description = "Не указаны точки для измерения."
    };


    /// <summary>
    /// Ошибка: команда КС не содержит ни одного параметра.
    /// </summary>
    public static ErrorItem EmptyCommandBody(int startLineNumber, string command) => new()
    {
      SourceLineNumber = startLineNumber,
      Command = command,
      Code = ErrorCode.Ks_EmptyCommandBody,
      Description = "Команда КС должна содержать хотя бы один параметр. Тело команды не может быть пустым."
    };

    public ErrorItem PairError(string command, string pointFirst, string pointLast) => new()
    {
      Command = command,
      Code = ErrorCode.Ks_PairError,
      Description = $"Замкнутая пара точек: {pointFirst}, {pointLast}"
    };

    public ErrorItem ChainPairError(string command, List<string> pointFirst, List<string> pointLast)
    {
      var eroror = new ErrorItem()
      {
        Command = command,
        Code = ErrorCode.Ks_PairError,
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

    public ErrorItem ChainError(string command, string chain) => new()
    {
      Command = command,
      Code = ErrorCode.Ks_ChainError,
      Description = $"Замкнутая цепь {chain}"
    };

    public ErrorItem DisconnectChainError(string command, string chain) => new()
    {
      Command = command,
      Code = ErrorCode.Ks_ChainError,
      Description = $"Разрыв в цепи {chain}"
    };

    public ErrorItem NodeExecutePointError(string command, List<string> point, string resultMeasure)
    {
      var error = new ErrorItem()
      {
        MeasureResult = resultMeasure,
        Command = command,
        Code = ErrorCode.Ks_NodeExecutePointError,
      };

      var str = string.Empty;
      foreach (var item in point)
      {
        str += $"#{item}";
      }

      error.Description = $"Ошибка при проверке цепи {str} при методе полного узла.";
      return error;
    }
  }
}
