using System.IO;

namespace NeutronMaintenance
{
    public interface IMasterMaintenanceProcessor
    {
        void ProcessFiles();
        void FileProcessor(FileInfo[] files);
        void ArchiveFile(FileInfo fileInfo);
    }
}