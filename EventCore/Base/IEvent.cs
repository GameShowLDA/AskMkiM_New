using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventCore.Base
{
  /// <summary>
  /// Базовый интерфейс для всех типов событий системы.
  /// Используется для унификации обработки и обеспечения типобезопасности при подписке и публикации.
  /// </summary>
  public interface IEvent { }
}
