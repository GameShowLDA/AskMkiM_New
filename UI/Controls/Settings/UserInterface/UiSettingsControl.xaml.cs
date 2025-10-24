using AppConfiguration.Parameter;
using DTO.SettingsModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UI.Localization;
using Utilities.Extensions;
using static AppConfiguration.Parameter.ParameterConfig;
using static EventCore.Events.ThemeEvent;

namespace UI.Controls.Settings.UserInterface
{
  /// <summary>
  /// Логика взаимодействия для UiSettingsControl.xaml
  /// </summary>
  public partial class UiSettingsControl : UserControl
  {
    SettingsParameterModel _baseParameterModel { get; set; }
    private record LangOption(string Key, string Title);
    private record ThemeOption(string Key, string Title);

    /// <summary>
    /// Глобальный флаг наличия несохранённых изменений в разделе.
    /// <para>True — есть отличия от сохранённой модели; False — всё совпадает.</para>
    /// </summary>
    public bool HasUnsavedChanges { get; private set; }

    public UiSettingsControl()
    {
      InitializeComponent();
      Loaded += UiSettingsControl_Loaded;
      Unloaded += UiSettingsControl_Unloaded;
      SyntaxHighlighting.CheckedChanged += (s, ev) => ValueChanged(s, ev);
    }

    /// <summary>
    /// Клик по галочке «сохранить»: сохраняет текущую модель,
    /// перечитывает базу и скрывает индикаторы изменений.
    /// </summary>
    private async void Success_PreviewMouseDown(object sender, MouseButtonEventArgs e) => await SaveData();

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

    public async Task SaveData()
    {
      await SaveProtocolModel(GetModel());
      _baseParameterModel = await GetParameterModel();

      Error.Visibility = Visibility.Collapsed;
      Success.Visibility = Visibility.Collapsed;
      HasUnsavedChanges = false;
    }

    private void ValueChanged(object? sender, object? e)
    {
      if (!ProtocolEquals(_baseParameterModel, GetModel()))
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
    }

    private async void UiSettingsControl_Loaded(object sender, RoutedEventArgs e)
    {
      _baseParameterModel = await GetParameterModel();

      LanguageSelect.ValueChanged += ValueChanged;
      ThemeSelect.ValueChanged += ValueChanged;

      Success.PreviewMouseDown += Success_PreviewMouseDown;
      Error.PreviewMouseDown += Error_PreviewMouseDown;

      Error.Visibility = Visibility.Collapsed;
      Success.Visibility = Visibility.Collapsed;
      HasUnsavedChanges = false;

      LoadLanguageOptions();
      LoadThemeOptions(_baseParameterModel.Theme);

      EventCore.Services.EventAggregator.Subscribe<EventCore.Events.ThemeEvent.Change>(OnThemeChanged);
    }

    private void UiSettingsControl_Unloaded(object sender, RoutedEventArgs e)
    {
      EventCore.Services.EventAggregator.Unsubscribe<EventCore.Events.ThemeEvent.Change>(OnThemeChanged);
    }

    /// <summary>
    /// Загружает список доступных языков интерфейса и устанавливает текущий.
    /// </summary>
    private void LoadLanguageOptions()
    {
      var cultures = LocalizationService.GetAvailableCultures();

      var options = cultures
        .Select(c => new LangOption(
            Key: c.Name,
            Title: LocalizationService.GetDisplayName(c)))
        .ToList();

      LanguageSelect.ItemsSource = options;

      var current = LanguageSettings.CurrentLanguage;
      LanguageSelect.DefaultValue = current;
      LanguageSelect.SelectedValue = current;
    }

    private void LoadThemeOptions(DTO.Enum.ThemeEnums.Theme currentTheme)
    {
      var themes = Enum.GetValues(typeof(DTO.Enum.ThemeEnums.Theme))
                       .Cast<DTO.Enum.ThemeEnums.Theme>()
                       .Select(t => new ThemeOption(t.ToString(), t.GetDisplayName()))
                       .ToList();

      ThemeSelect.ItemsSource = themes;
      var themeString = currentTheme.ToString();
      ThemeSelect.DefaultValue = themeString;
      ThemeSelect.SelectedValue = themeString;
    }

    /// <summary>
    /// Заполняет элементы UI значениями из базовой (сохранённой) модели.
    /// </summary>
    private void DefalultData()
    {
      var current = _baseParameterModel.Language;
      LanguageSelect.DefaultValue = current;
      LanguageSelect.SelectedValue = current;

      var currentTheme = _baseParameterModel.Theme.ToString();
      ThemeSelect.DefaultValue = currentTheme;
      ThemeSelect.SelectedValue = currentTheme;

      SyntaxHighlighting.IsChecked = _baseParameterModel.UseSyntaxHighlighting;
    }

    /// <summary>
    /// Формирует модель параметров из текущих значений элементов UI.
    /// </summary>
    private SettingsParameterModel GetModel()
    {
      var languageCode =
          LanguageSelect.SelectedValue as string
          ?? LanguageSelect.SelectedItem?.ToString()
          ?? LanguageSettings.CurrentLanguage;

      var themeValue = ThemeSelect.SelectedValue as string ?? "Dark";
      var parsedTheme = Enum.TryParse<DTO.Enum.ThemeEnums.Theme>(themeValue, out var theme) ? theme : DTO.Enum.ThemeEnums.Theme.Dark;

      return new SettingsParameterModel
      {
        Language = languageCode,
        Theme = parsedTheme,
        UseSyntaxHighlighting = SyntaxHighlighting.IsChecked
      };
    }


    /// <summary>
    /// Сравнивает две модели параметров.
    /// </summary>
    private static bool ProtocolEquals(SettingsParameterModel a, SettingsParameterModel b) =>
      a.Language == b.Language &&
      a.UseSyntaxHighlighting == b.UseSyntaxHighlighting &&
      b.Theme == a.Theme;

    /// <summary>
    /// Обработчик события смены темы. Вызывается, когда тема меняется глобально.
    /// </summary>
    private void OnThemeChanged(EventCore.Events.ThemeEvent.Change e)
    {
      Theme.ThemeManager.ApplyThemeAsync(e.NewTheme);

      ThemeSelect.DefaultValue = e.NewTheme.ToString();
      ThemeSelect.SelectedValue = e.NewTheme.ToString();
    }
  }
}
