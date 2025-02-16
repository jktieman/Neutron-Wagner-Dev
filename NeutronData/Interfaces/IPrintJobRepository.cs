using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeutronData.Models;

namespace NeutronData.Interfaces;
public interface IPrintJobRepository
{
    PrintJob GetPrintJob(int orderId);
    void Insert(PrintJob printJob);
    void Save();
    void Update(PrintJob printJob);
}
