using System.Threading.Tasks;

namespace NeutronLoader
{
    public interface IInterfaceProcessor
    {
        Task StartProcessingInterfaceFiles();
        void StopProcessingInterfaceFiles();
        Task RunLoaderOnce();
        void ErrorAlert(string err);
    }
}