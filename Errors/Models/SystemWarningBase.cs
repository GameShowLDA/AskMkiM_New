using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EventCore.Events.Message;

namespace Errors.Models
{
  /// <summary>
  /// Базовый тип исключения для всех системных компонентов проекта.
  /// Поддерживает структурированное описание ошибки через <see cref="WarningItem"/>.
  /// </summary>
  public class SystemWarningBase : Exception
  {
    /// <summary>
    /// Объект ошибки, содержащий код и описание.
    /// </summary>
    public WarningItem Warning { get; }

    /// <summary>
    /// Код ошибки, если он указан.
    /// </summary>
    public WarningCode? Code => Warning?.Code;

    /// <summary>
    /// Описание ошибки.
    /// </summary>
    public string Description => Warning?.Description ?? "Неизвестная ошибка.";

    /// <summary>
    /// Инициализирует новое системное исключение с объектом ошибки.
    /// </summary>
    /// <param name="warning">Объект ошибки, содержащий сведения о коде и описании.</param>
    public SystemWarningBase(WarningItem warning)
      : base(warning?.Description ?? "Неизвестная ошибка.")
    {
      Warning = warning ?? new WarningItem
      {
        Description = "Ошибка не определена.",
        Code = WarningCode.Unknown
      };
    }

    /// <summary>
    /// Инициализирует новое системное исключение по коду ошибки и описанию.
    /// </summary>
    /// <param name="code">Код ошибки.</param>
    /// <param name="description">Описание ошибки.</param>
    public SystemWarningBase(WarningCode code, string description)
      : base(description)
    {
      Warning = new WarningItem
      {
        Code = code,
        Description = description
      };
    }

    /// <summary>
    /// Инициализирует новое системное исключение с вложенным исключением.
    /// </summary>
    /// <param name="warning">Описание ошибки.</param>
    /// <param name="innerException">Вложенное исключение.</param>
    public SystemWarningBase(WarningItem warning, Exception innerException)
      : base(warning?.Description ?? "Ошибка системы.", innerException)
    {
      Warning = warning ?? new WarningItem
      {
        Description = "Ошибка не определена.",
        Code = WarningCode.Unknown
      };
    }

    /// <summary>
    /// Возвращает текстовое представление исключения в формате:
    /// [КОД] Описание.
    /// </summary>
    public override string ToString()
    {
      string code = Code?.ToString() ?? "UNKNOWN";
      return $"[{code}] {Description}";
    }
  }
}
