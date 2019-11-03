using System.Collections.Generic;
using System.IO;

namespace NeutronLoader
{
    internal interface IFileProcessor
    {
        void LoadFiles(List<FileInfo> files);
    }
}