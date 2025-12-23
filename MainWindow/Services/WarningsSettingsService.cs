using MainWindowProgram.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UI.Controls.Settings.Warnings;
using static UI.Components.Invoke.OpenFileButton;

namespace MainWindowProgram.Services
{
  /// <summary>
  /// Сервис управления настройками предупреждений.
  /// </summary>
  public sealed class WarningsSettingsService
  {
    private readonly MultiWindowService _multiWindow;
    private readonly WarningsSettingsStorage _storage;

    public WarningsSettingsService(
      MultiWindowService multiWindow,
      WarningsSettingsStorage storage)
    {
      _multiWindow = multiWindow;
      _storage = storage;
    }

    public async Task OpenWarningsSettingsAsync()
    {
      var vm = new WarningsSettingsViewModel(this);

      var control = new WarningsSettingsControl
      {
        DataContext = vm
      };

      await _multiWindow.AddControlAsync(
        "Предупреждения",
        control,
        TypeWindow.Settings);
    }

    // API для ViewModel
    public IReadOnlyList<WarningSetting> GetWarnings()
      => _storage.LoadAndSync();

    public void SaveWarnings(IEnumerable<WarningSetting> warnings)
      => _storage.Save(warnings);
  }
}
