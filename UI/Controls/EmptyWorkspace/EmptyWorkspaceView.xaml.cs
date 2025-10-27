using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using EventCore.Adapters;

namespace UI.Controls.EmptyWorkspace
{
  /// <summary>
  /// Логика взаимодействия для EmptyWorkspaceView.xaml
  /// </summary>
  public partial class EmptyWorkspaceView : UserControl, INotifyPropertyChanged
  {
    private readonly DispatcherTimer _timer;
    private DateTime _currentDateTime;

    public DateTime CurrentDateTime
    {
      get => _currentDateTime;
      set
      {
        if (_currentDateTime != value)
        {
          _currentDateTime = value;
          PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentDateTime)));
        }
      }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public EmptyWorkspaceView()
    {
      InitializeComponent();

      // стартовые значения
      CurrentDateTime = DateTime.Now;

      // тикаем каждую секунду
      _timer = new DispatcherTimer(DispatcherPriority.Background)
      {
        Interval = TimeSpan.FromMilliseconds(100)
      };
      _timer.Tick += (_, __) => CurrentDateTime = DateTime.Now;
      _timer.Start();

      // корректный стоп при выгрузке
      Unloaded += (_, __) => _timer.Stop();

      try
      {
        var session = new DataBaseConfiguration.Services.SessionService().HasSessionWithTabsAsync().Result;
        if (!session)
        {
          GreetingBar.Visibility = Visibility.Collapsed;
        }
      }
      catch (Exception)
      {
      }
    }

    private void GreetingBar_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
      SessionEventAdapter.RaiseOpenSession();
      GreetingBar.Visibility = Visibility.Collapsed;
    }

  }
}
