using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Errors;
using Utilities.Models;

namespace AppConfiguration.Error.Translation
{
  public class IeErrors : IPointError
  {
    /// <summary>
    /// Ошибка: команда ИЕ не содержит ни одной границы емкости.
    /// </summary>
    public static ErrorItem EmptyCapacity(int startLineNumber, string command) => new()
    {
      SourceLineNumber = startLineNumber,
      Command = command,
      Code = ErrorCode.Ie_EmptyCapacity,
      Description = "Команда ИЕ должна содержать хотя бы одну из границ емкости. Емкость не может быть не задано."
    };

    /// <summary>
    /// Ошибка: не удалось распознать параметры.
    /// </summary>
    public static ErrorItem CannotParseParameters(string parameters, int startLineNumber, string command) => new()
    {
      SourceLineNumber = startLineNumber,
      Command = command,
      Code = ErrorCode.Ie_CannotParseParameters,
      Description = $"Не удалось распознать параметры: {parameters}"
    };

    /// <summary>
    /// Ошибка: не указаны точки для измерения.
    /// </summary>
    public static ErrorItem EmptyPoints(int startLineNumber, string command) => new()
    {
      SourceLineNumber = startLineNumber,
      Command = command,
      Code = ErrorCode.Ie_EmptyPoints,
      Description = "Не указаны точки для измерения."
    };


    /// <summary>
    /// Ошибка: команда ИЕ не содержит ни одного параметра.
    /// </summary>
    public static ErrorItem EmptyCommandBody(int startLineNumber, string command) => new()
    {
      SourceLineNumber = startLineNumber,
      Command = command,
      Code = ErrorCode.Ie_EmptyCommandBody,
      Description = "Команда ИЕ должна содержать хотя бы один параметр. Тело команды не может быть пустым."
    };

    public ErrorItem PairError(string command, string pointFirst, string pointLast) => new()
    {
      Command = command,
      Code = ErrorCode.Ie_PairError,
      Description = $"Замкнутая пара точек: {pointFirst}, {pointLast}"
    };

    public ErrorItem ChainPairError(string command, List<PointModel> pointFirst, List<PointModel> pointLast)
    {
      var eroror = new ErrorItem()
      {
        Command = command,
        Code = ErrorCode.Ie_PairError,
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
      Code = ErrorCode.Ie_ChainError,
      Description = $"Замкнутая цепь {chain}"
    };

    public ErrorItem DisconnectChainError(string command, string chain) => new()
    {
      Command = command,
      Code = ErrorCode.Ie_ChainError,
      Description = $"Разрыв в цепи {chain}"
    };

    public ErrorItem NodeExecutePointError(string command, List<PointModel> point, string resultMeasure)
    {
      var error = new ErrorItem()
      {
        MeasureResult = resultMeasure,
        Command = command,
        Code = ErrorCode.Ie_NodeExecutePointError,
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
