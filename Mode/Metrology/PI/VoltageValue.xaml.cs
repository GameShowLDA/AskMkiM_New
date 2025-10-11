using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Mode.Metrology.PI
{
  /// <summary>
  /// Логика взаимодействия для VoltageValue.xaml.
  /// </summary>
  public partial class VoltageValue : Window
  {
    /// <summary>
    /// Результат введённого напряжения.
    /// </summary>
    public string VoltageResult { get; private set; }

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="VoltageValue"/>.
    /// </summary>
    public VoltageValue()
    {
      InitializeComponent();
      this.Loaded += Window_Loaded;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      VoltageInput.Focus();
    }

    /// <summary>
    /// Ограничивает ввод только числовыми значениями для номера устройства.
    /// </summary>
    /// <param name="sender">Источник события.</param>
    /// <param name="e">Аргументы события ввода текста.</param>
    private void NumberDevice_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
      var textBox = sender as TextBox;
      if (textBox == null)
      {
        e.Handled = true;
        return;
      }

      // Заменяем точку на запятую при вводе
      if (e.Text == ".")
      {
        int caretIndex = textBox.CaretIndex;
        textBox.Text = textBox.Text.Insert(caretIndex, ",");
        textBox.CaretIndex = caretIndex + 1;
        e.Handled = true; // предотвращаем вставку точки
        return;
      }

      // Разрешаем только цифры и запятую
      e.Handled = !Regex.IsMatch(e.Text, @"^[0-9,]$");

      // Не допускаем вторую запятую
      if (!e.Handled && e.Text == "," && textBox.Text.Contains(","))
      {
        e.Handled = true;
      }
    }


    private void VoltageInput_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
      {
        SaveButton_Click(sender, e);
      }
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
      VoltageResult = VoltageInput.Text;
      this.DialogResult = true;
      this.Close();
    }
  }
}
