namespace DTO.Attributes.Attributes
{
  [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
  public class CommandOrganizationalAttribute : Attribute
  {
    /// <summary>
    /// Отображаемое имя команды или параметра.
    /// Используется в пользовательском интерфейсе или при генерации документации
    /// для более понятного представления поля.
    /// </summary>
    public string DisplayName { get; }

    public CommandOrganizationalAttribute(string displayName)
    {
      DisplayName = displayName;
    }
  }
}
