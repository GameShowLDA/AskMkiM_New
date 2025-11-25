using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.Settings.SettingsModels
{
  /// <summary>
  /// Модель настроек отображения информации об устройствах и элементах программы
  /// контроля в составе системы АСК-МКИ-М. Содержит параметры,
  /// определяющие, какие сведения выводятся оператору в интерфейсе.
  /// </summary>
  public class DeviceDisplaySettingsModel
  {
    [Key]
    public int Id { get; set; } = 1;

    /// <summary>
    /// Определяет, отображаются ли машинные адреса точек.
    /// Используется для сопоставления логических точек схемы с
    /// фактическими адресами каналов системы АСК-МКИ-М.
    /// </summary>
    public bool ShowMachineAddresses { get; set; }

    /// <summary>
    /// Определяет, отображается ли информация о подключении точек
    /// и шин на устройствах. Позволяет оператору видеть структуру
    /// коммутации и фактические соединения в системе.
    /// </summary>
    public bool ShowConnectionInfo { get; set; }

    /// <summary>
    /// Определяет, отображаются ли параметры, которые устанавливаются
    /// на устройства в ходе выполнения программы контроля
    /// (напряжения, токи, временные интервалы, режимы и другие
    /// значения, передаваемые исполнителем устройствам).
    /// </summary>
    public bool ShowDeviceExecutionParameters { get; set; }

    /// <summary>
    /// Определяет, отображаются ли результаты измерений,
    /// полученные от устройств во время выполнения программы контроля.
    /// Включает вывод значений, зафиксированных измерительными модулями.
    /// </summary>
    public bool ShowMeasurementResults { get; set; }
  }
}
