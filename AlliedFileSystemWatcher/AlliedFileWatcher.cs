using System;
using System.IO;

namespace AlliedFileSystemWatcher
{
    public class AlliedFileWatcher : IDisposable
    {
        private readonly FileSystemWatcher _watcher;

        public event EventHandler<FileInfoArgs> FileCreated;

        public AlliedFileWatcher(string path, string filter, bool includeSubdirectories)
        {
            Path = path + @"\";
            Filter = filter;
            IncludeSubdirectories = includeSubdirectories;
            _watcher = new FileSystemWatcher
            {
                Path = path
                , Filter = filter
                , IncludeSubdirectories = includeSubdirectories
            };
            _watcher.Created += OnCreated;
        }

        public void Start()
        {
            EnableRaisingEvents = true;
            _watcher.EnableRaisingEvents = EnableRaisingEvents;
        }

        public void Stop()
        {
            EnableRaisingEvents = false;
            _watcher.EnableRaisingEvents = EnableRaisingEvents;
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

        public string Filter { get; set; }

        public bool EnableRaisingEvents { get; set; }

        public bool IncludeSubdirectories { get; set; }

        public bool Watch { get; set; }
        
        public string Path { get; set; }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _watcher?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
