using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.SettingsModels
{
  public class SettingsProtocolModel
  {

    public int Id { get; set; } = 1;

    /// <summary>
    /// Отображение данных об устройстве в протоколе.
    /// </summary>
    public bool ShowDeviceInfo { get; set; }

    /// <summary>
    /// Флаг, указывающий, нужно ли сохранять протокол.
    /// </summary>
    public bool AutoSaveProtocol { get; set; }

    /// <summary>
    /// Флаг, указывающий, нужно ли печатать протокол.
    /// </summary>
    public bool AutoPrintProtocol { get; set; }

    /// <summary>
    /// Флаг, указывающий время выполнения операций.
    /// </summary>
    public bool DisplayOperationTime { get; set; }

    /// <summary>
    /// Флаг, указывающий на подробное отображение протокола.
    /// </summary>
    public bool ShowDetailedProtocol { get; set; }

    /// <summary>
    /// Флаг указывающий на вывод протокола в ПО.
    /// </summary>
    public bool ShowProtocolInSoftware { get; set; }

    /// <summary>
    /// Флаг, указывающий, нужно ли формировать протокол.
    /// </summary>
    public bool GenerateProtocol { get; set; }


    /// <summary>
    /// Базовый текст протокола (без ошибок).
    /// </summary>
    public string CleanTextProtocol { get; set; } =
@"Протокол($РЕЖИМ) от $ДАТА
проверки электрических параметров сборочной единицы $ОБОЗНАЧЕНИЕ Зав.N $НОМЕР
Цель проверки: проверка электрических параметров сборочной единицы на соответствие техническим условиям
Оборудование: установка контроля электромонтажа АСК-МКИ
Программа проверки: $ПРОГРАММА
  Время начала измерений: $НАЧАЛО
  Время окончания измерений: $КОНЕЦ
  Время выполнения: $ВРЕМЯ

Обрывов: не обнаружено
Замыканий: не обнаружено
Нарушений изоляции: не обнаружено

Заключение: Изделие $ОБОЗНАЧЕНИЕ Зав.N $НОМЕР
            соответствует требованиям КД

Исполнитель: $ИСПОЛНИТЕЛЬ

Представитель ОК: $ПРЕДСТАВИТЕЛЬ

Представитель заказчика (ВП): $ЗАКАЗЧИК";


    /// <summary>
    /// Базовый текст протокола (с ошибками).
    /// </summary>
    public string CleanTextErrorsProtocol { get; set; } =
@"Протокол($РЕЖИМ) от $ДАТА

Зав.N сборочной единицы: $НОМЕР

Заключение: Изделие $ОБОЗНАЧЕНИЕ $НАИМЕНОВАНИЕ
            Зав.N $НОМЕР $БРАК(не )соответствует требованиям КД

Исполнитель: $ИСПОЛНИТЕЛЬ

Представитель ОТК: $ПРЕДСТАВИТЕЛЬ

Представитель заказчика (ВП): $ЗАКАЗЧИК";


    /// <summary>
    /// Базовый текст протокола (с ошибками).
    /// </summary>
    public string ErrorTextProtocol { get; set; } = string.Empty;
  }
}
