using EventCore.Adapters;
using EventCore.Events;
using EventCore.Services;

namespace AppConfiguration
{
  static public class AdminConfig
  {
    static AdminConfig()
    {
      EventAggregator.Subscribe<SystemStateEvents.AdminRightsChanged>(e => IsAdmin = e.IsAdmin);
    }

    /// <summary>
    /// Флаг, указывающий, запущено ли приложение с правами администратора.
    /// </summary>
    static internal bool IsAdmin { get; set; }

    /// <summary>
    /// Флаг, указывающий перехват Exception в режиме админа.
    /// </summary>
    static public bool ErrorDebug { get; set; } = false;

    /// <summary>
    /// Асинхронно устанавливает статус прав администратора и уведомляет систему.
    /// </summary>
    /// <param name="enable">
    /// <see langword="true"/>, если запущено с правами администратора;
    /// <see langword="false"/> — если в обычном режиме.
    /// </param>
    public static async Task SetAdminRights(bool enable) =>
      await Task.Run(() => SystemStateEventAdapter.RaiseAdminRightsChanged(enable));

    /// <summary>
    /// Асинхронно возвращает текущий статус прав администратора.
    /// </summary>
    /// <returns>
    /// <see langword="true"/>, если приложение работает с правами администратора;
    /// <see langword="false"/> — если без них.
    /// </returns>
    public static async Task<bool> GetAdminRights() => await Task.Run(() => IsAdmin);
  }
}
