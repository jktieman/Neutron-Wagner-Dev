#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using AlliedFileSystemWatcher;
using AlliedLogger;
using JsonManager;
using NeutronCore;
using NeutronCore.Global;
using NeutronCore.Models;
using NeutronData.Models;
using NeutronData.ModelViews;
using NeutronEvents;

#endregion

namespace NeutronLoader
{
    public class InterfaceProcessorVidir : IInterfaceProcessor
    {
        private BlockingCollection<FileInfo> _interfaceFileQueue;
        private static BackgroundWorker _backgroundWorker;
        private DirectoryInfo _hostOrderDirectory;
        private string _inputFileFilter;

        private AlliedFileWatcher _interfaceWatcher;
        private readonly IDynamicLogger _logger;
        private readonly NeutronVariables _neutronVariables;
        private readonly NeutronLicense _neutronLicense;
        private readonly IJsonData _jsonData;
        private readonly WorkstationView _workstationView;
        private VidirFileProcessor _fileProcessor;
        private const string FolderName = "Neutron Loader";

        public InterfaceProcessorVidir(NeutronVariables neutronVariables, NeutronLicense neutronLicense, IJsonData jsonData
            , WorkstationView workstationView)
        {
            Initialize();            
            _neutronVariables = neutronVariables;
            _neutronLicense = neutronLicense;
            _jsonData = jsonData;
            _workstationView = workstationView;
            _logger = NeutronCore.Global.Logger.SetupLogger("InterfaceProcessor");
        }

        private void Initialize()
        {
            _interfaceFileQueue = new BlockingCollection<FileInfo>();
            LoaderSettings.Init();
            _hostOrderDirectory = new DirectoryInfo(LoaderSettings.GetHostOrderDirectory());
            _inputFileFilter = LoaderSettings.GetHostOrderFileFilter();
            _fileProcessor = new VidirFileProcessor(_neutronVariables, _neutronLicense,_jsonData, _workstationView);
        }

        public void ErrorAlert(string err)
        {
            Mediator.GetInstance().OnLoaderError(this, err);
        }
        public void StartProcessingInterfaceFiles()
        {
            InitBackgroundWorker();

            // get any existing files first
            LoadOrders();

            _interfaceWatcher = new AlliedFileWatcher(_hostOrderDirectory.ToString(), _inputFileFilter, includeSubdirectories: false);
            _interfaceWatcher.FileCreated += FileCreated;
            _interfaceWatcher.Start();
        }

        private void LoadOrders()
        {
            _ = _logger.LogDetailAsync("Load Orders");
            try
            {
                var files = GetFiles();

                foreach (var file in files)
                {
                    _ = _logger.LogDetailAsync($"Add File to Interface File Queue: {file.FullName} ");

                    _interfaceFileQueue.TryAdd(file);

                    Thread.Sleep(100);
                }
            }
            catch (ObjectDisposedException oex)
            {
                _ = _logger.LogDetailAsync($"LoadOrders Object Disposed Exception {Environment.NewLine} {oex.Message}");
            }
            catch (InvalidOperationException iex)
            {
                _ = _logger.LogDetailAsync($"LoadOrders Invalid Operation Exception {Environment.NewLine} {iex.Message}");
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync($"LoadOrders Exception {Environment.NewLine} {ex.Message}");
            }
        }

        public void StopProcessingInterfaceFiles()
        {
            _interfaceWatcher.Stop();
            _backgroundWorker.CancelAsync();
            _interfaceFileQueue.CompleteAdding();
        }

        public void RunLoaderOnce()
        {
            throw new NotImplementedException();
        }

        public void RunLoaderContinuously()
        {
            throw new NotImplementedException();
        }

        public FileInfo[] GetFiles()
        {
            _ = _logger.LogDetailAsync("Call to Get Files Function.");
            var result = new FileInfo[] { };
            try
            {
                result = _hostOrderDirectory.GetFiles(_inputFileFilter);
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync(
                    $"Get Files Error.  {Environment.NewLine} {ex.Message} {Environment.NewLine} {ex.InnerException?.Message} {Environment.NewLine}  {ex.InnerException?.InnerException?.Message}");
            }
            return result;
        }

        private void InitBackgroundWorker()
        {
            _backgroundWorker = new BackgroundWorker
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = true
            };
            _backgroundWorker.DoWork += BackgroundWorkerDoWork;
            _backgroundWorker.RunWorkerCompleted += BackgroundWorkerRunWorkerCompleted;
            _backgroundWorker.RunWorkerAsync();
        }

        private void BackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            _ = _logger.LogDetailAsync("Queue Processor Do Work");
            if (_backgroundWorker.CancellationPending)
            {
                _ = _logger.LogDetailAsync($"BackgroundWorker Cancel.");
                e.Cancel = true;
                return;
            }

            Thread.Sleep(millisecondsTimeout: 100);
            foreach (var fileInfo in _interfaceFileQueue.GetConsumingEnumerable())
            {
                _ = _logger.LogDetailAsync($"Queue Processor Do Work: {fileInfo.FullName} License: {_neutronLicense.CompanyCode} ");
                var files = new List<FileInfo>();
                if (!File.Exists(fileInfo.FullName)) continue;
                files.Add(fileInfo);
                Thread.Sleep(millisecondsTimeout: 100);

                _ = _logger.LogDetailAsync($"VidirFileProcessor: Number of Files: {files.Count}");
               _fileProcessor.LoadFiles(files);
            }
        }

        private void BackgroundWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
            }
            else
            {
                object result = e.Result;
            }
        }

        internal void FileCreated(object sender, FileInfoArgs e)
        {
          _ = _logger.LogDetailAsync($"File Created: {e.FileInfo.FullName} ");
          _interfaceFileQueue.TryAdd(e.FileInfo);
        }
    }
}