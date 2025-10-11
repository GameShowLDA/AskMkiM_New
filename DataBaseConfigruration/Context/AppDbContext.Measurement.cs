using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO.Base.Models.MeasurementError;
using Microsoft.EntityFrameworkCore;

namespace DataBaseConfiguration.Context
{
  public partial class AppDbContext
  {
    /// <summary>
    /// Таблица погрешностей измерений.
    /// Содержит базовые сведения о типах команд и связанных с ними характеристиках погрешности.
    /// </summary>
    public DbSet<MeasurementErrorEntity> MeasurementErrors { get; set; }

    /// <summary>
    /// Таблица диапазонов погрешностей измерений.
    /// Хранит коллекцию диапазонов для каждой записи из <see cref="MeasurementErrors"/>,
    /// включая границы измерений и соответствующие процентные и абсолютные погрешности.
    /// </summary>
    public DbSet<MeasurementErrorRangeEntity> MeasurementErrorRanges { get; set; }
  }
}
