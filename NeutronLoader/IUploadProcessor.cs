using System.Threading.Tasks;

namespace NeutronLoader
{
    public interface IUploadProcessor
    {
        void StartProcessingUploadFiles();
        void StopProcessingUploadFiles();
        Task RunUploadOnce();
    }
}
