using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

/// <summary>
/// Аргументы события изменения точки останова.
/// </summary>
public sealed class BreakpointChangedEventArgs : EventArgs
{
  /// <summary>Номер строки, на которой произошли изменения.</summary>
  public int LineNumber { get; }

  /// <summary>true, если точка останова установлена; false, если снята.</summary>
  public bool IsSet { get; }

  public BreakpointChangedEventArgs(int lineNumber, bool isSet)
  {
    LineNumber = lineNumber;
    IsSet = isSet;
  }
}

/// <summary>
/// Левый марджин AvalonEdit для отображения:
/// - активной строки (зелёная точка),
/// - точек останова (красные точки).
/// </summary>
public sealed class ExecutionGlyphMargin : AbstractMargin
{
  private readonly TextEditor _textEditor;
  private readonly HashSet<int> _breakpoints = new();

  /// <summary>Кисть для маркера активной строки.</summary>
  public Brush ActiveLineBrush { get; set; } = Brushes.LimeGreen;

  /// <summary>Кисть для точки останова.</summary>
  public Brush BreakpointBrush { get; set; } = Brushes.Red;

  /// <summary>Текущая активная строка, либо null, если маркер не установлен.</summary>
  public int? ActiveLine { get; private set; }

  /// <summary>Текущий набор строк, на которых установлены точки останова.</summary>
  public IReadOnlyCollection<int> Breakpoints => _breakpoints;

  /// <summary>Событие вызывается при изменении точки останова.</summary>
  public event EventHandler<BreakpointChangedEventArgs>? BreakpointChanged;

  /// <summary>
  /// Разрешать ли клик по маргину (добавление/удаление точек останова).
  /// </summary>
  public bool IsClickEnabled { get; set; } = true;

  public ExecutionGlyphMargin(TextEditor textEditor)
  {
    _textEditor = textEditor ?? throw new ArgumentNullException(nameof(textEditor));
  }

  private void RunOnUi(Action action)
  {
    if (Dispatcher.CheckAccess())
      action();
    else
      Dispatcher.Invoke(action);
  }

  /// <summary>
  /// Установить маркер активной строки и прокрутить к ней редактор.
  /// </summary>
  public void SetActiveLine(int lineNumber)
  {
    RunOnUi(() =>
    {
      if (ActiveLine == lineNumber)
        return;

      ActiveLine = lineNumber;
      InvalidateVisual();
      _textEditor.ScrollTo(lineNumber, 1);
    });
  }

  /// <summary>
  /// Очистить маркер активной строки.
  /// Метод оставлен с прежним именем для совместимости.
  /// </summary>
  public void ClearMarkers()
  {
    RunOnUi(() =>
    {
      if (ActiveLine == null)
        return;

      ActiveLine = null;
      InvalidateVisual();
    });
  }

  /// <summary>
  /// Установить или снять точку останова на указанной строке.
  /// </summary>
  public void SetBreakpoint(int lineNumber, bool enabled = true)
  {
    RunOnUi(() =>
    {
      bool changed = enabled
        ? _breakpoints.Add(lineNumber)
        : _breakpoints.Remove(lineNumber);

      if (!changed)
        return;

      InvalidateVisual();
      BreakpointChanged?.Invoke(this, new BreakpointChangedEventArgs(lineNumber, enabled));
    });
  }

  /// <summary>
  /// Переключить точку останова на строке.
  /// </summary>
  public void ToggleBreakpoint(int lineNumber)
  {
    RunOnUi(() =>
    {
      bool isSet;
      if (_breakpoints.Contains(lineNumber))
      {
        _breakpoints.Remove(lineNumber);
        isSet = false;
      }
      else
      {
        _breakpoints.Add(lineNumber);
        isSet = true;
      }

      InvalidateVisual();
      BreakpointChanged?.Invoke(this, new BreakpointChangedEventArgs(lineNumber, isSet));
    });
  }

  /// <summary>
  /// Удалить все точки останова.
  /// </summary>
  public void ClearAllBreakpoints()
  {
    RunOnUi(() =>
    {
      if (_breakpoints.Count == 0)
        return;

      _breakpoints.Clear();
      InvalidateVisual();
    });
  }

  protected override Size MeasureOverride(Size availableSize) => new(20, 0);

  protected override void OnRender(DrawingContext drawingContext)
  {
    base.OnRender(drawingContext);

    if (TextView is not { VisualLinesValid: true })
      return;

    TextView.EnsureVisualLines();
    var visualLines = TextView.VisualLines;
    if (visualLines.Count == 0)
      return;

    double verticalOffset = TextView.ScrollOffset.Y;
    const double centerX = 10.0;

    foreach (var visualLine in visualLines)
    {
      int lineNumber = visualLine.FirstDocumentLine.LineNumber;
      double top = visualLine.VisualTop;
      double centerY = top - verticalOffset + visualLine.Height / 2.0;

      // Точка останова
      if (_breakpoints.Contains(lineNumber))
      {
        drawingContext.DrawEllipse(
          BreakpointBrush,
          null,
          new Point(centerX, centerY),
          8.0,
          8.0);
      }

      // Активная строка
      if (ActiveLine == lineNumber)
      {
        drawingContext.DrawEllipse(
          ActiveLineBrush,
          null,
          new Point(centerX, centerY),
          8.0,
          8.0);
      }
    }
  }

  /// <summary>
  /// Делаем марджин кликабельным, чтобы получать события мыши.
  /// </summary>
  protected override HitTestResult HitTestCore(PointHitTestParameters parameters) =>
    new PointHitTestResult(this, parameters.HitPoint);

  protected override void OnMouseDown(MouseButtonEventArgs e)
  {
    if (!IsClickEnabled) return;

    base.OnMouseDown(e);

    if (e.ChangedButton != MouseButton.Left || TextView == null)
      return;

    TextView.EnsureVisualLines();
    var visualLines = TextView.VisualLines;
    if (visualLines.Count == 0)
      return;

    Point posInTextView = e.GetPosition(TextView);
    double y = posInTextView.Y + TextView.ScrollOffset.Y;

    foreach (var visualLine in visualLines)
    {
      double top = visualLine.VisualTop;
      double bottom = top + visualLine.Height;

      if (y >= top && y <= bottom)
      {
        int lineNumber = visualLine.FirstDocumentLine.LineNumber;
        ToggleBreakpoint(lineNumber);
        e.Handled = true;
        break;
      }
    }
  }

  protected override void OnTextViewChanged(TextView? oldTextView, TextView? newTextView)
  {
    if (oldTextView != null)
      oldTextView.ScrollOffsetChanged -= TextView_ScrollOffsetChanged;

    if (newTextView != null)
      newTextView.ScrollOffsetChanged += TextView_ScrollOffsetChanged;

    base.OnTextViewChanged(oldTextView, newTextView);
  }

  private void TextView_ScrollOffsetChanged(object? sender, EventArgs e) =>
    InvalidateVisual();
}