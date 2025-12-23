using MainWindowProgram.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UI.Controls.Settings.Warnings;

namespace MainWindowProgram.ViewModels
{
  /// <summary>
  /// ViewModel настроек отображения предупреждений.
  /// </summary>
  /// <summary>
  /// ViewModel настроек предупреждений.
  /// </summary>
  public sealed class WarningsSettingsViewModel
  {
    private readonly WarningsSettingsService _service;

    public ObservableCollection<WarningSetting> Warnings { get; }

    public WarningsSettingsViewModel(WarningsSettingsService service)
    {
      _service = service;

      Warnings = new ObservableCollection<WarningSetting>(
        _service.GetWarnings());

      Warnings.CollectionChanged += (_, __) =>
        _service.SaveWarnings(Warnings);

      foreach (var item in Warnings)
        item.PropertyChanged += (_, e) =>
        {
          if (e.PropertyName == nameof(WarningSetting.IsEnabled))
            _service.SaveWarnings(Warnings);
        };
    }
  }
}
