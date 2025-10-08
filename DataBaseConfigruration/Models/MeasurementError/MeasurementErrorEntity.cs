using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppConfiguration.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataBaseConfiguration.Models.MeasurementError
{
  /// <summary>
  /// Тип команды (режим метрологии) и связанные с ним диапазоны погрешностей.
  /// </summary>
  public class MeasurementErrorEntity
  {
    /// <summary>Первичный ключ.</summary>
    public int Id { get; set; }

    /// <summary>Тип команды, для которой задаются погрешности.</summary>
    public TypeCommand Type { get; set; }

    /// <summary>Коллекция диапазонов погрешностей (один-ко-многим).</summary>
    public List<MeasurementErrorRangeEntity> Ranges { get; set; } = new();

    /// <summary>Пустой конструктор для EF.</summary>
    public MeasurementErrorEntity() { }

    /// <summary>Удобный конструктор с указанием типа команды.</summary>
    public MeasurementErrorEntity(TypeCommand type) => Type = type;
  }
}
