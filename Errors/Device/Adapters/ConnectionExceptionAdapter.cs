using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Errors.Device.Adapters
{
  internal static class ConnectionExceptionAdapter
  {
    /// <summary>  
    /// Исключение при ошибке подключения устройства.  
    /// </summary>  
    public static DeviceException? ConnectFailed(string name, int chassis, int number, string reason = null)
    {
      return new($"Ошибка подключения к {name}({chassis}.{number}){Format(reason)}");
    }

    /// <summary>  
    /// Исключение при ошибке отключения устройства.  
    /// </summary>  
    public static DeviceException? DisconnectFailed(string name, int chassis, int number, string reason = null)
    {
      return new($"Ошибка отключения от {name}({chassis}.{number}){Format(reason)}");
    }

    /// <summary>  
    /// Исключение при ошибке инициализации устройства.  
    /// </summary>  
    public static DeviceException? InitializeFailed(string name, int chassis, int number, string reason = null)
    {
      return new($"Ошибка инициализации {name}({chassis}.{number}){Format(reason)}");
    }

    /// <summary>  
    /// Исключение при ошибке сброса устройства.  
    /// </summary>  
    public static DeviceException? ResetFailed(string name, int chassis, int number, string reason = null)
    {
      return new($"Ошибка сброса {name}({chassis}.{number}){Format(reason)}");
    }

    /// <summary>  
    /// Форматирует дополнительное сообщение.  
    /// </summary>  
    private static string Format(string reason) =>
        string.IsNullOrWhiteSpace(reason) ? string.Empty : $": {reason}";
  }
}
