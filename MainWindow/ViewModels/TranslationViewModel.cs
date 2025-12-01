using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MainWindowProgram.Services;

namespace MainWindowProgram.ViewModels
{
  /// <summary>
  /// ViewModel для работы с переводом (сборкой и запуском программ контроля).
  /// </summary>
  public partial class TranslationViewModel : ObservableObject
  {
    private readonly TranslationServices _service;

    /// <summary>
    /// Создаёт новый экземпляр <see cref="TranslationViewModel"/>.
    /// </summary>
    public TranslationViewModel(TranslationServices service)
    {
      _service = service;
    }

    /// <summary>
    /// Команда запуска сборки программы контроля.
    /// </summary>
    [RelayCommand]
    private async Task BuildAsync() => await _service.BuildAsync();

    /// <summary>
    /// Команда запуска исполнителя программы контроля.
    /// </summary>
    [RelayCommand]
    private async Task RunAsync()
    {
      //await AppConfiguration.Execution.ExecutionConfig.SetBreakpointsMode(false);
      await AppConfiguration.Execution.ExecutionConfig.SetStepByStepMode(false);
      await _service.RunAsync();
    }

    /// <summary>
    /// Команда запуска исполнителя программы контроля.
    /// </summary>
    [RelayCommand]
    private async Task RunStepByStepModeAsync()
    {
      //await AppConfiguration.Execution.ExecutionConfig.SetBreakpointsMode(false);
      await AppConfiguration.Execution.ExecutionConfig.SetStepByStepMode(true);
      await _service.RunAsync();
    }

    //[RelayCommand]
    //private async Task RunStopPointsModeAsync()
    //{
    //  await AppConfiguration.Execution.ExecutionConfig.SetBreakpointsMode(true);
    //  await AppConfiguration.Execution.ExecutionConfig.SetStepByStepMode(false);
    //  await _service.RunWithBreakpointsAsync();
    //}
  }
}
