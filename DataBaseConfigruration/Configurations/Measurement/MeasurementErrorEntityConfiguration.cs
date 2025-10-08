using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataBaseConfiguration.Models.Hotkey;
using DataBaseConfiguration.Models.MeasurementError;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataBaseConfiguration.Configurations.Measurement
{
  /// <summary>Конфигурация таблицы MeasurementErrors.</summary>
  internal class MeasurementErrorEntityConfiguration : IEntityTypeConfiguration<MeasurementErrorEntity>
  {
    public void Configure(EntityTypeBuilder<MeasurementErrorEntity> builder)
    {
      builder.HasKey(x => x.Id);

      builder.Property(x => x.Type)
             .IsRequired();

      builder.HasIndex(x => x.Type).IsUnique();

      builder.HasMany(x => x.Ranges)
             .WithOne(x => x.MeasurementErrorEntity)
             .HasForeignKey(x => x.MeasurementErrorEntityId)
             .OnDelete(DeleteBehavior.Cascade);
    }
  }
}
