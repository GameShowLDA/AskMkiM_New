using AppConfiguration.Protocol;
using static AppConfiguration.Protocol.ProtocolConfig;

namespace Mode.Settings.ProtocolManager
{
  public partial class ProtocolManagerControl
  {
    /// <summary>
    /// Флаг, указывающий, начата ли работа с конфигурацией.
    /// </summary>
    readonly bool start = false;

    /// <summary>
    /// Устанавливает конфигурацию протокола на основе данных из YAML-файла.
    /// </summary>
    private async void SetConfiguration()
    {
      deviceData.IsChecked = await GetDeviceInfo();
      save.IsChecked = await GetSaveProtocol();
      print.IsChecked = await GetPrintProtocol();
      startTime.IsChecked = await GetTimeStart();
      showDetailedProtocol.IsChecked = await GetShowDetailedProtocol();
    }

    /// <summary>
    /// Сохраняет новые данные конфигурации протокола.
    /// </summary>
    private async Task NewDataSaveAsync()
    {
      if (start)
      {

        SettingsProtocolModel setProtocolModel = new()
        {
          ShowDeviceInfo = (bool)deviceData.IsChecked,
          AutoSaveProtocol = (bool)save.IsChecked,
          AutoPrintProtocol = (bool)print.IsChecked,
          DisplayOperationTime = (bool)startTime.IsChecked,
          ShowDetailedProtocol = (bool)showDetailedProtocol.IsChecked
        };

        await SaveProtocolModel(setProtocolModel);
      }
    }
  }
}
