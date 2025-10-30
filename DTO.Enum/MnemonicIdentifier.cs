using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.Enum
{
  /// <summary>
  /// Универсальный идентификатор мнемоники, поддерживающий как строковые,
  /// так и типизированные команды (MeasurementTypeCommand, OrganizationalComands и др.).
  /// </summary>
  public readonly struct MnemonicIdentifier
  {
    /// <summary>
    /// Строковое значение мнемоники.
    /// </summary>
    public string Mnemonic { get; }

    /// <summary>
    /// Общий enum-объект (если команда представлена перечислением).
    /// </summary>
    public System.Enum? EnumValue { get; }

    /// <summary>
    /// Конструктор для строковой мнемоники.
    /// </summary>
    public MnemonicIdentifier(string mnemonic)
    {
      Mnemonic = mnemonic;
      EnumValue = null;
    }

    public static implicit operator MnemonicIdentifier(string mnemonic)
      => new MnemonicIdentifier(mnemonic);

    /// <summary>
    /// Конструктор для команд измерений (MeasurementTypeCommand).
    /// </summary>
    public MnemonicIdentifier(Measurement.MeasurementTypeCommand command)
    {
      EnumValue = command;
      Mnemonic = command.ToString();
    }

    /// <summary>
    /// Конструктор для организационных команд (OrganizationalComands).
    /// </summary>
    public MnemonicIdentifier(Measurement.OrganizationalComands command)
    {
      EnumValue = command;
      Mnemonic = command.ToString();
    }

    /// <summary>
    /// Возвращает строковое представление мнемоники.
    /// </summary>
    public override string ToString() => Mnemonic ?? string.Empty;

    /// <summary>
    /// Проверяет, является ли команда типом MeasurementTypeCommand.
    /// </summary>
    public bool IsMeasurementTypeCommand => EnumValue is Measurement.MeasurementTypeCommand;

    /// <summary>
    /// Проверяет, является ли команда типом OrganizationalComands.
    /// </summary>
    public bool IsOrganizationalCommand => EnumValue is Measurement.OrganizationalComands;

    /// <summary>
    /// Возвращает MeasurementTypeCommand, если доступен; иначе — null.
    /// </summary>
    public Measurement.MeasurementTypeCommand? AsMeasurementTypeCommand =>
        EnumValue as Measurement.MeasurementTypeCommand?;

    /// <summary>
    /// Возвращает OrganizationalComands, если доступен; иначе — null.
    /// </summary>
    public Measurement.OrganizationalComands? AsOrganizationalCommand =>
        EnumValue as Measurement.OrganizationalComands?;
  }
}
