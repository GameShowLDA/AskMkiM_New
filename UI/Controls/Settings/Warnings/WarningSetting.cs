using Errors.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UI.Controls.Settings.Warnings
{
  /// <summary>
  /// Настройка отображения предупреждения.
  /// </summary>
  public sealed class WarningSetting : INotifyPropertyChanged
  {
    private bool _isEnabled = true;

    public WarningCode Code { get; set; }

    public string Tag { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public bool IsEnabled
    {
      get => _isEnabled;
      set
      {
        if (_isEnabled == value)
          return;

        _isEnabled = value;
        OnPropertyChanged();
      }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(
      [CallerMemberName] string? propertyName = null)
    {
      PropertyChanged?.Invoke(
        this,
        new PropertyChangedEventArgs(propertyName));
    }
  }
}
