using CommunityToolkit.Mvvm.Input;
using MainWindowProgram.Services;

namespace MainWindowProgram.ViewModels
{
  /// <summary>
  /// ViewModel для управления настройками приложения.
  /// Содержит команды для открытия различных вкладок с параметрами конфигурации, выполнения и протоколирования.
  /// </summary>
  public partial class SettingsViewModel
  {
    private readonly SettingsService _service;
    public WarningsSettingsViewModel Warnings { get; }

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="SettingsViewModel"/>.
    /// </summary>
    public SettingsViewModel(SettingsService service, WarningsSettingsService warningsService)
    {
      _service = service;
      Warnings = new WarningsSettingsViewModel(warningsService);
    }

    /// <summary>Открыть раздел "Общие сведения" в справочнике.</summary>
    [RelayCommand]
    private async Task HelpOpenGeneralInformation() => await _service.HelpOpenGeneralInformation();

    /// <summary>Открыть раздел "Язык программ контроля" в справочнике.</summary>
    [RelayCommand]
    private async Task HelpOpenLanguageControlPrograms() => await _service.HelpOpenLanguageControlPrograms();

    /// <summary>Открыть раздел "Состав программы" в справочнике.</summary>
    [RelayCommand]
    private async Task HelpOpenProgramComposition() => await _service.HelpOpenProgramComposition();

    /// <summary>Открыть раздел "О программе" в справочнике.</summary>
    [RelayCommand]
    private async Task HelpOpenAboutProgram() => await _service.HelpOpenAboutProgram();

    /// <summary>Открыть быстрое меню команд.</summary>
    [RelayCommand]
    private async Task HelpOpenFastMenu() => await _service.HelpOpenFastMenuCommand();

    /// <summary>Открыть общие настройки приложения.</summary>
    [RelayCommand]
    private async Task Settings() => await _service.OpenSettingsAsync();

    [RelayCommand]
    private async Task OpenWarningsSettings() => await _service.OpenWarningsSettingsAsync();
  }
}
