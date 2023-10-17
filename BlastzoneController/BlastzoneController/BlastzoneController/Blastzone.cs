using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlastzoneController
{
    public class Blastzone : HardwareDevice, IBlastzone
    {

        public Blastzone(HardwareDevice hardwareDevice)
        {
            Init();
        }

        private void Init()
        {
            
        }
    }
}
