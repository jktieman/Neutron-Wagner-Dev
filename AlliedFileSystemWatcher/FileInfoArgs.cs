using System;
using System.IO;
using System.Linq;

namespace AlliedFileSystemWatcher
{
    public class FileInfoArgs : EventArgs
    {
        private FileInfo fileInfo;

        public FileInfoArgs(FileInfo fileInfo)
        {
            this.fileInfo = fileInfo;
        }

        public FileInfo FileInfo
        {
            get {
                return fileInfo;
            }
            set {
                fileInfo = value;
            }
        }
    }
}
