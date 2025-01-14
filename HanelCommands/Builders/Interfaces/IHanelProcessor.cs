using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanelCommands.Builders.Interfaces
{
    public interface IHanelProcessor
    {
        void Process(byte[] dataIn, ref List<HanelDeviceStatus> hanelDeviceStatusList);
    }
}
