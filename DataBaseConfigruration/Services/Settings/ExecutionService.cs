using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppConfiguration.Execution;
using AppConfiguration.Protocol;
using Microsoft.EntityFrameworkCore;

namespace DataBaseConfiguration.Services.Settings
{
  public class ExecutionService
  {
    /// <summary>
    /// Сохраняет настройки протокола в БД.
    /// Если строки ещё нет, создаёт новую.
    /// Если строка есть, обновляет её.
    /// </summary>
    public async Task SaveExecutionAsync(ExecutionModel value)
    {
      using var db = DataBaseConfig.Context;

      var existing = await db.Set<ExecutionModel>().FirstOrDefaultAsync();
      if (existing == null)
      {
        await db.Set<ExecutionModel>().AddAsync(value);
      }
      else
      {
        db.Entry(existing).CurrentValues.SetValues(value);
      }

      await db.SaveChangesAsync();
    }

    /// <summary>
    /// Возвращает сохранённые настройки протокола.
    /// Если строки нет, возвращает null.
    /// </summary>
    public async Task<ExecutionModel?> GetExecutionAsync()
    {
      using var db = DataBaseConfig.Context;
      return await db.Set<ExecutionModel>().FirstOrDefaultAsync();
    }
  }
}
