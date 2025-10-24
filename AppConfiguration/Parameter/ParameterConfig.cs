using DTO.Base.Models;
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

    public static async Task SetSyntaxHighlighting(bool enable)
    {
      await Task.Run(() =>
      {
        ParameterModel.UseSyntaxHighlighting = enable;
      });
    }

    #endregion

    #region Get.

    /// <summary>
    /// Возвращает язык интерфейса программы.
    /// </summary>
    /// <returns>true, если отображается; false, если скрывается.</returns>
    public static async Task<string> GetLanguage() =>  ParameterModel.Language;
    public static async Task<ThemeEnums.Theme> GetTheme() => ParameterModel.Theme;
    public static bool GetSyntaxHighlighting() => ParameterModel.UseSyntaxHighlighting;


    public static async Task<SettingsParameterModel> GetParameterModel()
    {
      return await Task.Run(() =>
      {
        SettingsParameterModel parametrModel = new SettingsParameterModel();
        parametrModel.Language = ParameterModel.Language;
        parametrModel.Theme = ParameterModel.Theme;
        parametrModel.UseSyntaxHighlighting = ParameterModel.UseSyntaxHighlighting;
        return parametrModel;
      });
    }

    public static async Task SaveProtocolModel(SettingsParameterModel parametrModel)
    {
      await Task.Run(() =>
      {
        ParameterModel.Language = parametrModel.Language;
        ParameterModel.Theme = parametrModel.Theme;
        ParameterModel.UseSyntaxHighlighting = parametrModel.UseSyntaxHighlighting;

      });

      await RewriteExecutionConfigAsync();
      await LanguageSettings.SetLanguageAsync(ParameterModel.Language);
      await ThemeSettings.SetThemeAsync(ParameterModel.Theme);
      EventCore.Adapters.ThemeEventAdapter.RaiseSyntaxHighlighting(parametrModel.UseSyntaxHighlighting);
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
