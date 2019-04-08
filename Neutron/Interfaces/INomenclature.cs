using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neutron.Interfaces
{
    public interface INomenclature
    {
        string MBPickAccept { get; set; }
        string MBStoreAccept { get; set; }
        string MBDelete { get; set; }
        string LabelDevice { get; set; }
        string LabelTray { get; set; }
        string LabelOver { get; set; }
        string LabelBack { get; set; }
    }
}
