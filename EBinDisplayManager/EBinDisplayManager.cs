using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace EBinDisplay
{
    public class EBinDisplayManager
    {
        public EBinDisplayManager()
        {
        }

        public string TurnOnDisplay(int deviceNumber, int trayNumber, int level, int part, int quantity = 0,
            string display = "")
        {
            return $"Turn On EBin Location: Level:{level}  Over: {part}";
        }
    }
}
