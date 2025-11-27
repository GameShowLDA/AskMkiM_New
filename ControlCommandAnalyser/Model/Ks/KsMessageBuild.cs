using ControlCommandAnalyser.Model.Chains;
using ControlCommandAnalyser.Model.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlCommandAnalyser.Model.Ks
{
  public class KsMessageBuild : IDislpayInfo
  {
    public async Task<string> BuildErrorChainStringAsync(ChainModel chain)
    {
      var chainStr = string.Empty;

      for (int z = 0; z < chain.PointModels.Count; z++)
      {
        var pointErr = chain.PointModels[z];
        var machineAdrees = await AppConfiguration.DeviceDisplay.DeviceDisplayConfig.GetMachineAddressVisibilityAsync() ? $" [{pointErr.ToString()}]" : string.Empty;

        chainStr += pointErr.Mnemonic + machineAdrees;

        if (z + 1 < chain.PointModels.Count)
        {
          chainStr += ", ";
        }
      }

      return chainStr;
    }
  }
}
