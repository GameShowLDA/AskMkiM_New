using AppConfiguration.Base;
using DTO.Settings.SettingsModels;
using DTO.SettingsModels;

namespace AppConfiguration.Parameter
{
  static public class ParameterSettingsManager
  {
    /// <summary>
    /// Считывает параметры отображения данных в протоколе и задаёт их в программе.
    /// </summary>
    static public async Task ReadParameterModeAsync()
    {
      ParameterFileManager protocolFileManager = new ParameterFileManager(FileLocations.ParameterConfigPath);

      if (!await protocolFileManager.CreateFileIfNotExistsAsync())
      {
        return;
      }

      UserInterfaceModel protocolModel = await protocolFileManager.ReadFileAsync();
      if (protocolModel == null)
      {
        return;
      }

      await ConfigModel.SerParametrModelAsync(protocolModel);
    }
  }
}
