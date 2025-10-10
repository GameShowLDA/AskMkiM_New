using static DTO.Enum.HotKey;

namespace DTO.Base
{
  /// <summary>
  /// Унифицированный интерфейс горячей клавиши.
  /// </summary>
  public interface IHotkeyBinding
  {
    string ActionName { get; }
    string KeyCombination { get; }
    bool IsEnabled { get; }
    HotkeyScope Scope { get; }
    string? Description { get; }
  }
}
