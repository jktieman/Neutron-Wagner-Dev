using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPTI.Models
{
    public interface ITcpIptiCommandCenter
    {
        void AddBayController(BayController bayController);
        BayController GetBayController(string bayId);
        string TurnOnDisplay(string bayId, int displayId, int quantity);
        string ClearBayController(string bayId);
        string ClearOrderControlModule(string bayId);
        void LoadBayControllers();
    }
}
