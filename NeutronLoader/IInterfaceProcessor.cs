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