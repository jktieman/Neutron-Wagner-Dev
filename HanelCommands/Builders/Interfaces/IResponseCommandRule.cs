using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanelCommands.Builders.Interfaces
{
    public interface IResponseCommandRule
    {
        bool IsMatch(string command);
        void UpdateStatus(string[] commandSegments, ref List<HanelDeviceStatus> hanelDeviceStatusList);
    }
}
