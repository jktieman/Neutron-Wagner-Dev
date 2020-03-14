using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronLoader
{
    public interface IUploadProcessor
    {
        void StartProcessingUploadFiles();
        void StopProcessingUploadFiles();
    }
}
