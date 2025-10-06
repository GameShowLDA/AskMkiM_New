using AppConfiguration.Base;
using System.Windows;
using System.Windows.Input;
using Message;
using Utilities.ResultProtocol;


namespace UI.Controls.ProtocolNew
{
  /// <summary>
  /// Логика взаимодействия для ProtocolInfoWindow.xaml
  /// </summary>
  public partial class ProtocolInfoWindow : Window
  {

    /// <summary>
    /// Результат введённого заводского номера.
    /// </summary>
    public string NumberResult { get; private set; }

    /// <summary>
    /// Результат ввода исполнителя.
    /// </summary>
    public string ExecutorResult { get; private set; }

    /// <summary>
    /// Результат ввода представителя ОК.
    /// </summary>
    public string AgentResult { get; private set; }

    /// <summary>
    /// Результат ввода представителя заказчика (ВП).
    /// </summary>
    public string CustomerAgentResult { get; private set; }

    private ProtocolModel _protocolModel = null;
    public ProtocolInfoWindow(ProtocolModel protocolModel) : this()
    {
      _protocolModel = protocolModel;
    }

    public ProtocolInfoWindow()
    {
      InitializeComponent();
      this.Loaded += Window_Loaded;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      NumberInput.Text = string.Empty;
      ExecutorInput.Text = string.Empty;
      AgentInput.Text = string.Empty;
      CustomerAgentInput.Text = string.Empty;
      NumberInput.Focus();
    }

    private void Input_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
      {
        SaveButton_Click(sender, e);
      }
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
      NumberResult = NumberInput.Text;
      ExecutorResult = ExecutorInput.Text;
      AgentResult = AgentInput.Text;
      CustomerAgentResult = CustomerAgentInput.Text;
      if (string.IsNullOrEmpty(NumberResult)
        || string.IsNullOrEmpty(ExecutorResult)
        || string.IsNullOrEmpty(AgentResult)
        || string.IsNullOrEmpty(CustomerAgentResult))
      {
        Message.MessageBoxCustom.Show("Пожалуйста, заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        NumberInput.Focus();
      }
      else
      {
        this.DialogResult = true;
        EventAggregator.RaiseProtocolInfoClose(NumberResult, ExecutorResult, AgentResult, CustomerAgentResult, _protocolModel);
        this.Close();
      }
    }
  }
}
