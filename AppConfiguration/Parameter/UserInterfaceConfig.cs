using DTO.Base.Models;
using DTO.Enum;
using DTO.Settings.SettingsModels;
using DTO.SettingsModels;

namespace AppConfiguration.Parameter
{
  public static class UserInterfaceConfig
  {
    static UserInterfaceModel UserInterfaceModel = new UserInterfaceModel();

    static public Action<UserInterfaceModel> SaveUserInterfaceEvent;


    #region Set.
    /// <summary>
    /// Устанавливает язык интерйефса программы.
    /// </summary>
    /// <param name="enable">true для отображения, false для скрытия.</param>
    public static async Task SetLanguage(string enable)
    {
      var lang = string.IsNullOrWhiteSpace(enable) ? "ru" : enable.ToLowerInvariant();
      UserInterfaceModel.Language = lang;
      await LanguageSettings.SetLanguageAsync(lang);
    }

    /// <summary>
    /// Устанавливает тему оформления интерфейса программы.
    /// </summary>
    /// <param name="theme">Название темы: "Light" или "Dark".</param>
    public static async Task SetTheme(DTO.Enum.ThemeEnums.Theme theme)
    {
      UserInterfaceModel.Theme = theme;
      EventCore.Adapters.ThemeEventAdapter.RaiseChangeTheme(theme);
    }

    public static async Task SetSyntaxHighlighting(bool enable)
    {
      UserInterfaceModel.UseSyntaxHighlighting = enable;
    }

    public static async Task SetUserInterfaceModel(UserInterfaceModel user)
    {
      await Task.Run(async () =>
      {
        await SetLanguage(user.Language);
        await SetTheme(user.Theme);
        await SetSyntaxHighlighting(user.UseSyntaxHighlighting);
      });
    }

    #endregion

    #region Get.

    /// <summary>
    /// Возвращает язык интерфейса программы.
    /// </summary>
    /// <returns>true, если отображается; false, если скрывается.</returns>
    public static async Task<string> GetLanguage() => UserInterfaceModel.Language;
    public static async Task<ThemeEnums.Theme> GetTheme() => UserInterfaceModel.Theme;
    public static bool GetSyntaxHighlighting() => UserInterfaceModel.UseSyntaxHighlighting;


    public static async Task<UserInterfaceModel> GetParameterModel()
    {
      UserInterfaceModel parametrModel = new UserInterfaceModel();
      parametrModel.Language = UserInterfaceModel.Language;
      parametrModel.Theme = UserInterfaceModel.Theme;
      parametrModel.UseSyntaxHighlighting = UserInterfaceModel.UseSyntaxHighlighting;
      return parametrModel;
    }

    public static async Task SaveProtocolModel(UserInterfaceModel parametrModel)
    {
      UserInterfaceModel.Language = parametrModel.Language;
      UserInterfaceModel.Theme = parametrModel.Theme;
      UserInterfaceModel.UseSyntaxHighlighting = parametrModel.UseSyntaxHighlighting;
      SaveUserInterfaceEvent?.Invoke(parametrModel);


      await LanguageSettings.SetLanguageAsync(UserInterfaceModel.Language);
      await ThemeSettings.SetThemeAsync(UserInterfaceModel.Theme);
      EventCore.Adapters.ThemeEventAdapter.RaiseSyntaxHighlighting(parametrModel.UseSyntaxHighlighting);
    }


    #endregion
  }
}
