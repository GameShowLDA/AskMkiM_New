using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBaseConfiguration.Models.Session
{
  public class UserSessionEntity
  {
    public int Id { get; set; } = 1; // Всегда одна запись
    public string JsonData { get; set; } = string.Empty;
    public DateTime SavedAt { get; set; } = DateTime.Now;
  }
}
