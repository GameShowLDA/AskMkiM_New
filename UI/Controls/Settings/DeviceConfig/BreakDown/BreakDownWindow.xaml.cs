using System.Windows;
using AppConfiguration;
using AppConfiguration.Error.DataBase;
using DataBaseConfiguration.Models.Device;
using DataBaseConfiguration.Services.Device;
using DTO.Device.Base;
using DTO.Device.Breakdown;
using UI.Controls.Settings.DeviceConfig.Base;
using UI.Controls.Settings.DeviceConfig.Base.BaseSettingsConfig;

namespace UI.Controls.Settings.DeviceConfig.BreakDown
{
  /// <summary>
  /// Логика взаимодействия для BreakDownWindow.xaml.
  /// </summary>
  public partial class BreakDownWindow : Window, IDataProcessor
  {
    /// <summary>
    /// Событие запроса закрытия окна.
    /// </summary>
    public event EventHandler RequestClose;

    /// <summary>
    /// Событие запроса сохранения данных устройства.
    /// </summary>
    public event EventHandler<BreakdownTesterEntity> RequestSave;

    /// <summary>
    /// Свойство, предоставляющее доступ к параметрам устройства.
    /// </summary>
    public DeviceBase Property => new DeviceBase(deviceSettingsWindow);

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="BreakDownWindow"/>.
    /// </summary>
    public BreakDownWindow()
    {
      InitializeComponent();
    }

    /// <summary>
    /// Обрабатывает данные устройства.
    /// </summary>
    /// <param name="device">Экземпляр устройства.</param>
    /// <param name="control">Элемент управления настройками устройства.</param>
    public void ProcessData(IDevice device, DeviceSettingsControl control)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Устанавливает настройки для пробойной установки.
    /// </summary>
    /// <param name="sender">Источник события.</param>
    /// <param name="e">Экземпляр головного устройства.</param>
    public void SetSettings(object? sender, IHeadUnit e)
    {
      deviceSettingsWindow.NameDevice = "Пробойная установка";
      deviceSettingsWindow.LoadDeviceModels<IBreakdownTester>();
      deviceSettingsWindow.SetHeadUnit(e);

      deviceSettingsWindow.SaveEvent += (s, a) =>
      {
        var processor = new DeviceSettingsProcessorBase();
        var baseDevice = deviceSettingsWindow.CreateSelectedDeviceInstance();

        BreakdownTesterEntity deviceEntity = processor.ProcessDevice<BreakdownTesterEntity>(
            selectedDevice: baseDevice as IDevice,
            control: deviceSettingsWindow,
            additionalDataProcessor: this);

        if (deviceEntity != null)
        {
          deviceEntity.MaxVoltage = (baseDevice as IBreakdownTester).MaxVoltage;
          var svc = ServiceLocator.GetRequired<BreakdownTesterServices>();

          try
          {
            svc.Create(deviceEntity);
            RequestSave?.Invoke(s, deviceEntity);
            Close();
          }
          catch (DuplicateEntityException ex)
          {
            var messsage = ex.Message;
            Message.MessageBoxCustom.Show(messsage, "Ошибка сохраненения данных", image: MessageBoxImage.Error);
            throw;
          }
        }

      };

      deviceSettingsWindow.RequestClose += (s, a) =>
      {
        RequestClose?.Invoke(s, a);
        Close();
      };
    }
  }
}
