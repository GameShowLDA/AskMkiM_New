using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.Base.Interface
{
  public interface IDockItem
  {
    public string Title { get; set; }
    public object Content { get; set; }
  }
}
