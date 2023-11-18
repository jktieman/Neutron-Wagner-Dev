using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanelCommands;

namespace Hanel_DC.Hanel_DeviceControllers
{
    internal class MachineRequestType
    {
        public readonly int TextActionShow = 1;
        public readonly int TextActionClear = -1;
        public int DeviceUnit { get; set; }
        public int Tray { get; set; }
        public int Facing { get; set; }
        public int Depth { get; set; }
        public int Quantity { get; set; }
        public int TextAction { get; set; }
        public string Text { get; set; }
        public HanelCommand HanelCommand { get; set; }

    }
}
