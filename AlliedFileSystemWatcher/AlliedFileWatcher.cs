using System;
using System.IO;

namespace AlliedFileSystemWatcher
{
    public class AlliedFileWatcher : IDisposable
    {
        private readonly FileSystemWatcher watcher;
        private string path;
        private string filter;
        private bool watch = false;
        private bool includeSubdirectories = false;
        private bool enableRaisingEvents = false;


        public event EventHandler<FileInfoArgs> FileCreated;

        public AlliedFileWatcher(string path, string filter, bool includeSubdirectories)
        {
            this.path = path + @"\";
            this.filter = filter;
            this.includeSubdirectories = includeSubdirectories;
            watcher = new FileSystemWatcher();
            watcher.Path = path;
            watcher.Filter = filter;
            watcher.IncludeSubdirectories = includeSubdirectories;
            watcher.Created += OnCreated;
        }

        public void Start()
        {
            enableRaisingEvents = true;
            watcher.EnableRaisingEvents = enableRaisingEvents;
        }

        public void Stop()
        {
            enableRaisingEvents = false;
            watcher.EnableRaisingEvents = enableRaisingEvents;
        }

        protected virtual void OnCreated(object sender, FileSystemEventArgs e)
        {
            if (FileCreated != null)
            {
                var fileInfo = new FileInfo(e.FullPath);
                var fileInfoArgs = new FileInfoArgs(fileInfo);

                FileCreated(sender: null, e: fileInfoArgs);
            }

        }

        public string Filter
        {
            get { return filter; }
            set { filter = value; }
        }

        public bool EnableRaisingEvents
        {
            get { return enableRaisingEvents; }
            set { enableRaisingEvents = value; }
        }


        public bool IncludeSubdirectories
        {
            get { return includeSubdirectories; }
            set { includeSubdirectories = value; }
        }


        public bool Watch
        {
            get { return watch; }
            set { watch = value; }
        }


        public string Path
        {
            get { return path; }
            set { path = value; }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (watcher != null)
                {
                    watcher.Dispose();
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


    }
}
