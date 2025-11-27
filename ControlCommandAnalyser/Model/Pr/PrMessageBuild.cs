using ControlCommandAnalyser.Model.Chains;
using ControlCommandAnalyser.Model.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlCommandAnalyser.Model.Pr
{
  internal class PrMessageBuild : IDislpayInfo
  {
    public async Task<string> BuildErrorChainStringAsync(ChainModel chain)
    {
      var chainStr = await PointFormater.GetFormatConnectPoint(chain);
      return chainStr;
    }
  }
}
