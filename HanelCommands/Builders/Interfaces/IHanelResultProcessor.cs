using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanelCommands.Builders.Interfaces
{
    public interface IHanelResultProcessor
    {
        void Process(string[] commandSegments, ref List<HanelDeviceStatus> hanelDeviceStatusList);
    }
}
