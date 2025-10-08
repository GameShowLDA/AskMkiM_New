using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppConfiguration.Execution;
using AppConfiguration.Protocol;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataBaseConfiguration.Configurations.Settings
{
  internal class SettingsExecutionConfig : IEntityTypeConfiguration<ExecutionModel>
  {
    public void Configure(EntityTypeBuilder<ExecutionModel> builder)
    {
      builder.HasKey(x => x.Id);
    }
  }
}
