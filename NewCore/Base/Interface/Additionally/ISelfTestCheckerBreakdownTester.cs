using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NewCore.Base.Interface.Main;
using Utilities.Interface;

namespace NewCore.Base.Interface.Additionally
{
  public interface ISelfTestCheckerBreakdownTester
  {
    /// <summary>
    /// Запуск самоконтроля устройства коммутации шин для выбранного типа проверки.
    /// </summary>
    /// <param name="messageService">Сервис отображения сообщений.</param>
    /// <param name="selectedType">Выбранное значение перечисления.</param>
    /// <param name="device">Устройство коммутации шин (необязательно).</param>
    /// <param name="meter">Измеритель (необязательно).</param>
    Task StartSelfCheck(CancellationToken cancellationToken, System.Enum selectedType, IUserMessageService? userMessageService = null, IBreakdownTester breakdownTester = null, ISwitchingDevice device = null, IFastMeter meter = null);

    /// <summary>
    /// Возвращает тип перечисления, используемый как тип проверки.
    /// </summary>
    Type GetTestTypeEnum();
  }
}
