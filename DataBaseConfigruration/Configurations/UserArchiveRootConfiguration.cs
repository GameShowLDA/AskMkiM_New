using DTO.Base.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataBaseConfiguration.Configurations
{
  internal class UserArchiveRootConfiguration : IEntityTypeConfiguration<UserArchiveRootEntity>
  {
    /// <summary>
    /// Настройки таблицы погрешности в базе данных.
    /// </summary>
    /// <param name="builder">Объект для настройки сущности настроек в базе данных.</param>
    public void Configure(EntityTypeBuilder<UserArchiveRootEntity> builder)
    {
      builder.HasKey(x => x.Id);

      builder.Property(x => x.DisplayName)
          .IsRequired()
          .HasMaxLength(256);

      builder.Property(x => x.FolderPath)
             .IsRequired()
             .HasMaxLength(1024);

      builder.HasIndex(x => x.FolderPath)
             .IsUnique();

      builder.Property(x => x.SearchRecursively)
             .HasDefaultValue(true);

      builder.Property(x => x.IsEncryptionPlanned)
             .HasDefaultValue(false);
    }
  }
}
