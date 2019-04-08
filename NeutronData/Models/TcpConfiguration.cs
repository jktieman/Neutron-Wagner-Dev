using NeutronData.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronData.Models
{
    public class TcpConfiguration :IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string IPAddress { get; set; }
        public int Port { get; set; }
        public int DeviceCount { get; set; }
        public int NotificationTimeout { get; set; }

    }
}
