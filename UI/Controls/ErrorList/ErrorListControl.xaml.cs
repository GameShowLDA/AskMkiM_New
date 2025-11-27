using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Errors.Models;
using EventCore.Events;
using EventCore.Services;
using Utilities.Help;

namespace UI.Controls.ErrorList
{
  /// <summary>
  /// Логика взаимодействия для ErrorListControl.xaml
  /// </summary>
  public partial class ErrorListControl : UserControl
  {
    public ObservableCollection<ErrorItem> Errors { get; } = new();
    public ErrorListControl()
    {
      InitializeComponent();
      DataContext = this;
      EventAggregator.Subscribe<SystemStateEvents.DebugRightsChanged>(e => DebugChanged(e.IsDebug));
      if (AppConfiguration.AdminConfig.GetDebugRights())
      { 
          DebugColumn.Visibility = Visibility.Visible;
      }

      // Регистрируем обработчик движения мыши
      MouseMove += (s, e) =>
      {
        // Обновляем последний элемент под курсором
        HelpProvider.SetHelpKey(this, "DescriptionWorkTranslator");
      };
    }

    private void DebugChanged(bool isDebug)
    {
      Application.Current.Dispatcher.Invoke(() =>
      {

        if (isDebug)
        {
          DebugColumn.Visibility = Visibility.Visible;
        }
        else
        {
          DebugColumn.Visibility = Visibility.Collapsed;
        }
      });
    }

    public Visibility StringsNumberVisible
    {
      get
      {
        return StringsNumber.Visibility;
      }
      set
      {
        StringsNumber.Visibility = value;
      }
    }

    public Visibility MeasureResultVisible
    {
      get
      {
        return MeasureResult.Visibility;
      }
      set
      {
        MeasureResult.Visibility = value;
      }
    }

    /// <summary>
    /// Событие вызывается при двойном клике по строке с ошибкой.
    /// </summary>
    public event Action<ErrorItem>? ErrorItemDoubleClicked;

    private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      if (sender is DataGrid grid && grid.SelectedItem is ErrorItem selectedError)
      {
        ErrorItemDoubleClicked?.Invoke(selectedError);
      }
    }
  }
}
