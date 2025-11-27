using EventCore.Interfaces;

namespace EventCore.Events
{
  /// <summary>
  /// События, связанные с выполнением алгоритмов и управлением режимами выполнения.
  /// </summary>
  /// <remarks>
  /// Эти события используются для отслеживания изменений состояния выполнения, таких как
  /// включение пошагового режима, переход в автоматический режим и другие связанные состояния.
  /// </remarks>
  public static class ExecutionEvents
  {
    /// <summary>
    /// Событие, генерируемое при изменении состояния пошагового режима выполнения алгоритма.
    /// </summary>
    /// <remarks>
    /// Событие уведомляет все заинтересованные компоненты о том, что режим пошагового выполнения был включён или выключен.
    /// </remarks>
    public class StepByStepModeChanged : IEvent
    {
      /// <summary>
      /// Показывает, активирован ли пошаговый режим.
      /// </summary>
      public bool IsEnabled { get; }

      /// <summary>
      /// Инициализирует новый экземпляр события изменения состояния пошагового режима.
      /// </summary>
      /// <param name="isEnabled">
      /// <see langword="true"/> — если пошаговый режим включён;  
      /// <see langword="false"/> — если режим выключен.
      /// </param>
      public StepByStepModeChanged(bool isEnabled)
      {
        IsEnabled = isEnabled;
      }
    }

    /// <summary>
    /// Событие, генерируемое при изменении состояния режима выполнения с точками остановки.
    /// </summary>
    /// <remarks>
    /// Событие уведомляет компоненты о том, что режим выполнения с использованием breakpoints был включён или выключен.
    /// Логически аналогично <see cref="StepByStepModeChanged"/>, но относится к отдельному режиму.
    /// </remarks>
    public sealed class BreakpointsModeChanged : IEvent
    {
      /// <summary>
      /// Показывает, активирован ли режим точек остановки.
      /// </summary>
      public bool IsEnabled { get; }

      /// <summary>
      /// Инициализирует новый экземпляр события изменения состояния режима точек остановки.
      /// </summary>
      /// <param name="isEnabled">
      /// <see langword="true"/> — если режим точек остановки включён;  
      /// <see langword="false"/> — если режим выключен.
      /// </param>
      public BreakpointsModeChanged(bool isEnabled)
      {
        IsEnabled = isEnabled;
      }
    }
  }
}
