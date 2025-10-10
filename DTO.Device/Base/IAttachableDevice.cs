using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.Device.Base
{
  /// <summary>
  /// Интерфейс устройства, которое можно подключать к шасси.
  /// Наследует основные свойства и методы устройства из <see cref="IDevice"/>.
  /// </summary>
  public interface IAttachableDevice : IDevice
  {
    /// <summary>
    /// Получает или задаёт номер менеджера шасси, к которому подключено устройство.
    /// </summary>
    int NumberChassis { get; set; }
  }
}
