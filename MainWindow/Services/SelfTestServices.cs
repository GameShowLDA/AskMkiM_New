using Mode.SelfControl.DeviceCheck;
using static UI.Components.Invoke.OpenFileButton;

namespace MainWindowProgram.Services
{
  public class SelfTestServices
  {
    /// <summary>
    /// Сервис управления многооконным интерфейсом.
    /// </summary>
    private readonly MultiWindowService _multiWindow;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="TestService"/>.
    /// </summary>
    /// <param name="multiWindow">Сервис управления многооконным интерфейсом.</param>
    public SelfTestServices(MultiWindowService multiWindow)
    {
      _multiWindow = multiWindow;
    }

    /// <summary>
    /// Добавляет элемент управления для теста методом узла СИ в multiEditors.
    /// </summary>
    public async Task AddSelfTestModuleAsync() =>
      await _multiWindow.AddControlAsync("Самоконтроль модуля", new ModuleSelfControl(), TypeWindow.DeviceControl);


    /// <summary>
    /// Добавляет элемент управления для теста методом узла СИ в multiEditors.
    /// </summary>
    public async Task AddSelfTestSystemAsync() =>
      await _multiWindow.AddControlAsync("Самоконтроль системы", new SystemSelfControl(), TypeWindow.DeviceControl);

  }
}
