using EventCore.Events;
using EventCore.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventCore.Adapters
{
  /// <summary>
  /// Адаптер для генерации событий <see cref="BreakpointEvents"/>,
  /// обеспечивающий единый стиль вызова публикации событий.
  /// </summary>
  public static class BreakpointEventAdapter
  {
    /// <summary>
    /// Генерирует событие установки точки останова на строку.
    /// </summary>
    public static void RaiseBreakpointSet(int lineNumber) =>
      EventAggregator.Publish(new BreakpointEvents.BreakpointSet(lineNumber));

    /// <summary>
    /// Генерирует событие снятия точки останова со строки.
    /// </summary>
    public static void RaiseBreakpointRemoved(int lineNumber) =>
      EventAggregator.Publish(new BreakpointEvents.BreakpointRemoved(lineNumber));
  }
}
