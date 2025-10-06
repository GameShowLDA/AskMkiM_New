using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using DataBaseConfiguration.Models.MeasurementError;
using DataBaseConfiguration.Models.Session;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataBaseConfiguration.Configurations
{
  internal class SessionConfiguration : IEntityTypeConfiguration<UserSessionEntity>
  {
    public void Configure(EntityTypeBuilder<UserSessionEntity> builder)
    {
      builder.HasKey(x => x.Id);
    }
  }
}
