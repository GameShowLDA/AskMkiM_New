using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBaseConfiguration.Models.Session
{
  public class EditorTabSession
  {
    public string FilePath { get; set; } = ""; // может быть пустым
    public string FileName { get; set; } = "Новый"; // отображаемое имя вкладки
    public string TextContent { get; set; } = "";
    public bool IsModified { get; set; }
  }
}
