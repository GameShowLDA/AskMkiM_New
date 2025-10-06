using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBaseConfiguration.Models.Session
{
  public class SessionModel
  {
    public List<EditorTabSession> Tabs { get; set; } = new();
    public int ActiveTabIndex { get; set; } = 0;
    public DateTime SavedAt { get; set; } = DateTime.Now;
  }
}
