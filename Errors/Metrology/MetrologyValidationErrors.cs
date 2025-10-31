using Errors.Models;

namespace Errors.Metrology
{
  /// <summary>
  /// Содержит стандартные исключения, возникающие при валидации метрологических данных —
  /// точек подключения, параметров времени, напряжения, шины и других входных данных.
  /// </summary>
  public static class MetrologyValidationErrors
  {
    /// <summary>
    /// Исключение: элемент ввода данных (InputField) не найден в пользовательском интерфейсе.
    /// </summary>
    public static SystemExceptionBase InputFieldNotFound() =>
      new(new ErrorItem
      {
        Code = ErrorCode.Metrology_Validation_InputFieldNotFound,
        Description = "Элемент ввода не найден."
      });

    /// <summary>
    /// Исключение: указана некорректная первая точка подключения.
    /// </summary>
    public static SystemExceptionBase InvalidFirstPoint() =>
      new(new ErrorItem
      {
        Code = ErrorCode.Metrology_Validation_InvalidFirstPointFormat,
        Description = "Неверный формат первой точки."
      });

    /// <summary>
    /// Исключение: указана некорректная вторая точка подключения.
    /// </summary>
    public static SystemExceptionBase InvalidSecondPoint() =>
      new(new ErrorItem
      {
        Code = ErrorCode.Metrology_Validation_InvalidSecondPointFormat,
        Description = "Неверный формат второй точки."
      });

    /// <summary>
    /// Исключение: электрический параметр должен быть числовым значением.
    /// </summary>
    public static SystemExceptionBase InvalidElectricalValue() =>
      new(new ErrorItem
      {
        Code = ErrorCode.Metrology_Validation_InvalidParameter,
        Description = "Не удалось распознать электрический параметр. Параметр должен быть целым или дробным числом(x.y)."
      });

    /// <summary>
    /// Исключение: указано некорректное значение напряжения.
    /// </summary>
    public static SystemExceptionBase InvalidVoltage() =>
      new(new ErrorItem
      {
        Code = ErrorCode.Metrology_Validation_InvalidVoltage,
        Description = "Значение напряжения должно быть числом вида x.y."
      });

    /// <summary>
    /// Исключение: указано некорректное значение времени выполнения или измерения.
    /// </summary>
    public static SystemExceptionBase InvalidTime() =>
      new(new ErrorItem
      {
        Code = ErrorCode.Metrology_Validation_InvalidTime,
        Description = "Не удалось распознать время. Параметр должен быть целым или дробным числом(x.y)."
      });

    /// <summary>
    /// Исключение: точка подключения выходит за пределы диапазона допустимых значений.
    /// </summary>
    public static SystemExceptionBase PointOutOfRange(int point, int maxPoint) =>
      new(new ErrorItem
      {
        Code = ErrorCode.Metrology_Validation_PointOutOfRange,
        Description = $"Точка {point} выходит за предел допустимого диапазона (1–{maxPoint})."
      });

    /// <summary>
    /// Исключение: шасси с указанным номером не найдено в конфигурации.
    /// </summary>
    public static SystemExceptionBase ChassisNotFound(int chassisNumber) =>
      new(new ErrorItem
      {
        Code = ErrorCode.Metrology_Validation_InvalidFirstPointFormat,
        Description = $"Шасси с номером {chassisNumber} не найдено."
      });

    /// <summary>
    /// Исключение: модуль коммутации не найден в указанном шасси.
    /// </summary>
    public static SystemExceptionBase ModuleNotFound(int chassisNumber, int moduleNumber) =>
      new(new ErrorItem
      {
        Code = ErrorCode.Metrology_Validation_InvalidSecondPointFormat,
        Description = $"Модуль {moduleNumber} в шасси {chassisNumber} не найден."
      });

    /// <summary>
    /// Исключение: точки подключения не уникальны (повторяются).
    /// </summary>
    /// <param name="first">Первая точка.</param>
    /// <param name="second">Вторая точка.</param>
    public static SystemExceptionBase PointsNotUnique(string first, string second) =>
      new(new ErrorItem
      {
        Code = ErrorCode.Metrology_Validation_PointsNotUnique,
        Description = $"Точки подключения {first} и {second} не уникальны. Проверьте правильность ввода."
      });

    /// <summary>
    /// Исключение: указана некорректная шина подключения.
    /// </summary>
    public static SystemExceptionBase InvalidBusSelection() =>
      new(new ErrorItem
      {
        Code = ErrorCode.Metrology_Validation_InvalidBus,
        Description = "Шина подключения указана некорректно. Проверьте выбранное значение."
      });
  }
}
