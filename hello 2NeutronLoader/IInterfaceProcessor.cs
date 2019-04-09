using System.Collections.Generic;
using System.IO;
using NeutronData.Models;

namespace NeutronLoader
{
    public interface IInterfaceProcessor
    {

        FileInfo[] GetFiles();
        void ProcessFiles(FileInfo[] files);
        List<HostOrder> ProcessInterfaceFile(FileInfo fileInfo);
        bool ProcessOrdersToNeutron(IList<HostOrder> hostOrderLines);
        void Put(IList<HostOrder> hostOrderLines);
        void StartProcessingInterfaceFiles();
        void StopProcessingInterfaceFiles();
    }
}