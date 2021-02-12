using System;
using System.IO;

namespace AlliedFileSystemWatcher
{
    public class FileInfoArgs : EventArgs
    {
        public FileInfo FileInfo { get; set; }

        public FileInfoArgs(FileInfo fileInfo)
        {
            FileInfo = fileInfo;
        }
    }
}
