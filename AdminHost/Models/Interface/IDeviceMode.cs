namespace AdminHost.Models.Interface
{
  /// <summary>
  /// Базовый интерфейс, описывающий любой режим устройства.
  /// </summary>
  public interface IDeviceMode
  {
    /// <summary>
    /// Имя режима (отображаемое пользователю).
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Подробное описание режима.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Тип устройства, для которого доступен этот режим (например, typeof(IBreakdownTester)).
    /// </summary>
    Type DeviceInterface { get; }
  }
}
