using System.Collections.Generic;
using System.IO;
using NeutronData.Models;

namespace NeutronLoader
{
    public interface IInterfaceProcessor
    {
        void StartProcessingInterfaceFiles();
        void StopProcessingInterfaceFiles();
        void RunLoaderOnce();
        void RunLoaderContinuously();
    }
}