using DTO.Enum;
using DTO.Settings.SettingsModels;
using DTO.SettingsModels;

namespace AppConfiguration.Parameter
{
  public static class ParameterConfig
  {
    static SettingsParameterModel ParameterModel = new SettingsParameterModel();


    /// <summary>
    /// Перезаписывает конфигурационный файл протокола с текущими настройками.
    /// </summary>
    public static async void RewriteProtocolConfig()
    {
      SettingsParameterModel protocolModel = new SettingsParameterModel();

      ParameterFileManager protocolFileManager = new ParameterFileManager(FileLocations.ParameterConfigPath);
      await protocolFileManager.RewriteFileAsync(protocolModel);
    }

    #region Set.
    /// <summary>
    /// Устанавливает язык интерйефса программы.
    /// </summary>
    /// <param name="enable">true для отображения, false для скрытия.</param>
    public static async Task SetLanguage(string enable)
    {
      await Task.Run(async () =>
      {
        var lang = string.IsNullOrWhiteSpace(enable) ? "ru" : enable.ToLowerInvariant();
        ParameterModel.Language = lang;
        await LanguageSettings.SetLanguageAsync(lang);
      });
    }

    /// <summary>
    /// Устанавливает тему оформления интерфейса программы.
    /// </summary>
    /// <param name="theme">Название темы: "Light" или "Dark".</param>
    public static async Task SetTheme(DTO.Enum.ThemeEnums.Theme theme)
    {
      await Task.Run(() =>
      {
        ParameterModel.Theme = theme;
        EventCore.Adapters.ThemeEventAdapter.RaiseChangeTheme(theme);
      });
    }

    #endregion

    #region Get.

    /// <summary>
    /// Возвращает язык интерфейса программы.
    /// </summary>
    /// <returns>true, если отображается; false, если скрывается.</returns>
    public static async Task<string> GetLanguage() => await Task.Run(() => ParameterModel.Language);
    public static async Task<ThemeEnums.Theme> GetTheme() => await Task.Run(() => ParameterModel.Theme);

    public static async Task<SettingsParameterModel> GetParameterModel()
    {
      return await Task.Run(() =>
      {
        SettingsParameterModel parametrModel = new SettingsParameterModel();
        parametrModel.Language = ParameterModel.Language;
        parametrModel.Theme = ParameterModel.Theme;
        return parametrModel;
      });
    }

    public static async Task SaveProtocolModel(SettingsParameterModel protocolModel)
    {
      await Task.Run(() =>
      {
        ParameterModel.Language = protocolModel.Language;
        ParameterModel.Theme = protocolModel.Theme;
      });

      await RewriteExecutionConfigAsync();
      await LanguageSettings.SetLanguageAsync(ParameterModel.Language);
      await ThemeSettings.SetThemeAsync(ParameterModel.Theme);
    }

    /// <summary>
    /// Перезаписывает конфигурацию выполнений.
    /// </summary>
    /// <returns></returns>
    public static async Task RewriteExecutionConfigAsync()
    {
      SettingsParameterModel executionModel = new SettingsParameterModel();
      executionModel.Language = ParameterModel.Language;
      executionModel.Theme = ParameterModel.Theme;

      ParameterFileManager executionFileManager = new ParameterFileManager(FileLocations.ParameterConfigPath);
      await executionFileManager.RewriteFileAsync(executionModel);
    }

    #endregion
  }
}
