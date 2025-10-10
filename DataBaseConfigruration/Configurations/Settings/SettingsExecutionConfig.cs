using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppConfiguration.Execution;
using AppConfiguration.Protocol;
using DTO.SettingsModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataBaseConfiguration.Configurations.Settings
{
  internal class SettingsExecutionConfig : IEntityTypeConfiguration<SettingsExecutionModel>
  {
    public void Configure(EntityTypeBuilder<SettingsExecutionModel> builder)
    {
      builder.HasKey(x => x.Id);
    }
  }
}
