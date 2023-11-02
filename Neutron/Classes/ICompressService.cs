namespace Neutron.Classes
{
    public interface ICompressService
    {
        bool CompressRunning { get; }
        void StartCompressService();
    }
}