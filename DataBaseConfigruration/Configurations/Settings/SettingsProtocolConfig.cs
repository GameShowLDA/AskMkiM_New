using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppConfiguration.Protocol;
using DataBaseConfiguration.Models.Session;
using DTO.SettingsModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataBaseConfiguration.Configurations.Settings
{
  internal class SettingsProtocolConfig : IEntityTypeConfiguration<SettingsProtocolModel>
  {
    public void Configure(EntityTypeBuilder<SettingsProtocolModel> builder)
    {
      builder.HasKey(x => x.Id);
    }
  }
}
