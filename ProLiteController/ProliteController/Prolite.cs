using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProliteController
{
    public class Prolite : IProlite
    {
        private readonly SerialPort _serialPort;

        public Prolite(SerialPort serialPort)
        {
            _serialPort = serialPort;
            Init();
        }

        private void Init()
        {

        }
    }
}
