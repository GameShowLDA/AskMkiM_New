using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AppConfiguration.Protocol;
using DTO.SettingsModels;
using static AppConfiguration.Protocol.ProtocolConfig;

namespace UI.Controls.Settings.Protocol
{
  /// <summary>
  /// Контрол раздела «Протокол»: отображает настройки и отслеживает несохранённые изменения.
  /// Показывает иконки «✔/✖» при отличии текущих значений от сохранённой (базовой) модели.
  /// </summary>
  public partial class ProtocolControl : UserControl
  {
    /// <summary>
    /// Базовая (сохранённая) модель протокола, считанная при загрузке.
    /// Используется как эталон для сравнения с текущими значениями UI.
    /// </summary>
    private SettingsProtocolModel _baseProtocolModel { get; set; }

    /// <summary>
    /// Создаёт экземпляр контрола протокола.
    /// </summary>
    public ProtocolControl()
    {
      InitializeComponent();
      Loaded += ProtocolControl_Loaded;
    }

    /// <summary>
    /// Глобальный флаг наличия несохранённых изменений в разделе.
    /// <para>True — есть отличия от сохранённой модели; False — всё совпадает.</para>
    /// </summary>
    public bool HasUnsavedChanges { get; private set; }

    /// <summary>
    /// Обработчик события загрузки контрола.
    /// Подгружает сохранённую модель, заполняет UI и подписывается на изменения.
    /// </summary>
    private async void ProtocolControl_Loaded(object sender, RoutedEventArgs e)
    {
      _baseProtocolModel = await GetProtocolModel();
      DefalultData();

      DeviceInfo.CheckedChanged += CheckedChanged;
      AutoSave.CheckedChanged += CheckedChanged;
      AutoPrint.CheckedChanged += CheckedChanged;
      OperationTime.CheckedChanged += CheckedChanged;
      ProtocolFromPO.CheckedChanged += CheckedChanged;
      ProtocolGeneration.CheckedChanged += CheckedChanged;
      BaseTextProtocol.TextChanged += (s, ev) => CheckedChanged(s, true);

      Success.PreviewMouseDown += Success_PreviewMouseDown;
      Error.PreviewMouseDown += Error_PreviewMouseDown;

      Error.Visibility = Visibility.Collapsed;
      Success.Visibility = Visibility.Collapsed;
      HasUnsavedChanges = false;

      if (BaseTextProtocol.Text != await ProtocolConfig.GetBaseTextProtocol())
      {
        RestartClearProtocol.Visibility = Visibility.Visible;
      }
      else
      {
        RestartClearProtocol.Visibility = Visibility.Collapsed;
      }
    }

    /// <summary>
    /// Клик по галочке «сохранить»: сохраняет текущую модель,
    /// перечитывает базу и скрывает индикаторы изменений.
    /// </summary>
    private async void Success_PreviewMouseDown(object sender, MouseButtonEventArgs e) => await SaveData();
    public async Task SaveData()
    {
      await SaveProtocolModel(GetModel());
      _baseProtocolModel = await GetProtocolModel();

      Error.Visibility = Visibility.Collapsed;
      Success.Visibility = Visibility.Collapsed;
      HasUnsavedChanges = false;
    }

    /// <summary>
    /// Клик по кресту «отменить»: откатывает значения к сохранённой модели
    /// и скрывает индикаторы изменений.
    /// </summary>
    private void Error_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
      DefalultData();

      Error.Visibility = Visibility.Collapsed;
      Success.Visibility = Visibility.Collapsed;
      HasUnsavedChanges = false;
    }

    /// <summary>
    /// Унифицированный обработчик изменений любого переключателя.
    /// Сравнивает текущую модель с сохранённой и показывает/скрывает индикаторы.
    /// </summary>
    private async void CheckedChanged(object? sender, bool e)
    {
      if (!ProtocolEquals(_baseProtocolModel, GetModel()))
      {
        Error.Visibility = Visibility.Visible;
        Success.Visibility = Visibility.Visible;
        HasUnsavedChanges = true;
      }
      else
      {
        Error.Visibility = Visibility.Collapsed;
        Success.Visibility = Visibility.Collapsed;
        HasUnsavedChanges = false;
      }

      if (BaseTextProtocol.Text != await ProtocolConfig.GetBaseTextProtocol())
      {
        RestartClearProtocol.Visibility = Visibility.Visible;
      }
      else
      {
        RestartClearProtocol.Visibility = Visibility.Collapsed;
      }
    }

    /// <summary>
    /// Формирует модель протокола из текущих значений элементов UI.
    /// </summary>
    private SettingsProtocolModel GetModel()
    {
      var model = new SettingsProtocolModel
      {
        ShowDeviceInfo = DeviceInfo.IsChecked,
        AutoSaveProtocol = AutoSave.IsChecked,
        AutoPrintProtocol = AutoPrint.IsChecked,
        DisplayOperationTime = OperationTime.IsChecked,
        ShowProtocolInSoftware = ProtocolFromPO.IsChecked,
        GenerateProtocol = ProtocolGeneration.IsChecked,
        CleanTextProtocol = BaseTextProtocol.Text,
      };

      return model;
    }

    /// <summary>
    /// Сравнивает две модели протокола по всем флагам.
    /// </summary>
    private static bool ProtocolEquals(SettingsProtocolModel a, SettingsProtocolModel b) =>
      a.ShowDeviceInfo == b.ShowDeviceInfo &&
      a.AutoSaveProtocol == b.AutoSaveProtocol &&
      a.AutoPrintProtocol == b.AutoPrintProtocol &&
      a.ShowProtocolInSoftware == b.ShowProtocolInSoftware &&
      a.GenerateProtocol == b.GenerateProtocol &&
      a.CleanTextProtocol == b.CleanTextProtocol &&
      a.DisplayOperationTime == b.DisplayOperationTime;

    /// <summary>
    /// Заполняет элементы UI значениями из базовой (сохранённой) модели.
    /// </summary>
    private void DefalultData()
    {
      DeviceInfo.IsChecked = _baseProtocolModel.ShowDeviceInfo;
      AutoSave.IsChecked = _baseProtocolModel.AutoSaveProtocol;
      AutoPrint.IsChecked = _baseProtocolModel.AutoPrintProtocol;
      OperationTime.IsChecked = _baseProtocolModel.DisplayOperationTime;
      ProtocolFromPO.IsChecked = _baseProtocolModel.ShowProtocolInSoftware;
      ProtocolGeneration.IsChecked = _baseProtocolModel.GenerateProtocol;
      BaseTextProtocol.Text = _baseProtocolModel.CleanTextProtocol;
    }

    private async void RepeatIcon_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
      BaseTextProtocol.Text = await ProtocolConfig.GetBaseTextProtocol();
      RestartClearProtocol.Visibility = Visibility.Collapsed;
    }
  }
}
