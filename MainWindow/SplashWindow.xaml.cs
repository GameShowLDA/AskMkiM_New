using System.Windows;
using System.Windows.Media.Animation;

namespace MainWindowProgram
{
  /// <summary>
  /// Окно заставки (SplashScreen), отображаемое при запуске приложения.
  /// </summary>
  public partial class SplashWindow : Window
  {
    private Task? _closeTask;

    /// <summary>
    /// Инициализирует новый экземпляр окна заставки.
    /// </summary>
    public SplashWindow()
    {
      InitializeComponent();
    }

    /// <summary>
    /// Асинхронно выполняет плавное закрытие окна с анимацией исчезновения.
    /// </summary>
    /// <returns>Задача, представляющая процесс закрытия окна.</returns>
    public Task WaitForCloseAsync()
    {
      if (_closeTask != null)
        return _closeTask;

      if (!Dispatcher.CheckAccess())
        return Dispatcher.InvokeAsync(WaitForCloseAsync).Task.Unwrap();

      bool shutDown = Dispatcher.HasShutdownStarted || Dispatcher.HasShutdownFinished;
      bool skipAnimation = !IsLoaded || !IsVisible;

      if (shutDown || skipAnimation)
      {
        if (!shutDown) Close();
        return _closeTask = Task.CompletedTask;
      }

      var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
      _closeTask = tcs.Task;

      var fadeOut = new DoubleAnimation
      {
        To = 0.0,
        Duration = TimeSpan.FromMilliseconds(600),
        FillBehavior = FillBehavior.HoldEnd
      };

      fadeOut.Completed += (_, __) =>
      {
        Close();
        tcs.TrySetResult(true);
      };

      BeginAnimation(OpacityProperty, fadeOut);
      return _closeTask;
    }
  }
}