#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AlliedFileSystemWatcher;
using AlliedLogger;
using AsyncAwaitBestPractices;
using JsonManager;
using NeutronCore;
using NeutronCore.Global;
using NeutronCore.Models;
using NeutronData.DataContexts;
using NeutronData.ModelViews;
using NeutronEvents;

#endregion

namespace NeutronLoader
{
    public class InterfaceProcessorTop : IInterfaceProcessor
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
        private TopFileProcessor _fileProcessor;
        private readonly Func<NeutronDb> _contextFactory;
        private const string FolderName = "Neutron Loader";

        public InterfaceProcessorTop(NeutronVariables neutronVariables, NeutronLicense neutronLicense,
            IJsonData jsonData, WorkstationView workstationView, Func<NeutronDb> contextFactory)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
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
            _fileProcessor = new TopFileProcessor(_neutronVariables, _neutronLicense,_jsonData, _workstationView, _contextFactory);
        }

        public void ErrorAlert(string err)
        {
            Mediator.GetInstance().OnLoaderError(this, err);
        }
        public async Task StartProcessingInterfaceFiles()
        {
            InitBackgroundWorker();

            // get any existing files first
           await LoadOrders();

            _interfaceWatcher = new AlliedFileWatcher(_hostOrderDirectory.ToString(), _inputFileFilter, includeSubdirectories: false);
            _interfaceWatcher.FileCreated += FileCreated;
            _interfaceWatcher.Start();
        }

        private async Task LoadOrders()
        {
             _logger.LogDetailAsync("Load Orders").SafeFireAndForget();
            try
            {
                var files = GetFiles();

                foreach (var file in files)
                {
                    _logger.LogDetailAsync($"Add File to Interface File Queue: {file.FullName} ").SafeFireAndForget();

                    _interfaceFileQueue.TryAdd(file);

                    await Task.Delay(100);
                }
            }
            catch (ObjectDisposedException oex)
            {
                _logger.LogDetailAsync($"LoadOrders Object Disposed Exception {Environment.NewLine} {oex.Message}").SafeFireAndForget();
            }
            catch (InvalidOperationException iex)
            {
                _logger.LogDetailAsync($"LoadOrders Invalid Operation Exception {Environment.NewLine} {iex.Message}").SafeFireAndForget();
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"LoadOrders Exception {Environment.NewLine} {ex.Message}").SafeFireAndForget();
            }
        }

        public void StopProcessingInterfaceFiles()
        {
            _interfaceWatcher.Stop();
            _backgroundWorker.CancelAsync();
            _interfaceFileQueue.CompleteAdding();
        }

        public async Task RunLoaderOnce() => await LoadOrders();


        public void RunLoaderContinuously()
        {
            throw new NotImplementedException();
        }

        public FileInfo[] GetFiles()
        {
            _logger.LogDetailAsync("Call to Get Files Function.").SafeFireAndForget();
            var result = new FileInfo[] { };
            try
            {
                result = _hostOrderDirectory.GetFiles(_inputFileFilter);
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync(
                    $"Get Files Error.  {Environment.NewLine} {ex.Message} {Environment.NewLine} {ex.InnerException?.Message} {Environment.NewLine}  {ex.InnerException?.InnerException?.Message}").SafeFireAndForget();
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
            _logger.LogDetailAsync("Queue Processor Do Work").SafeFireAndForget();
            if (_backgroundWorker.CancellationPending)
            {
                _logger.LogDetailAsync($"BackgroundWorker Cancel.").SafeFireAndForget();
                e.Cancel = true;
                return;
            }

            Thread.Sleep(millisecondsTimeout: 100);
            foreach (var fileInfo in _interfaceFileQueue.GetConsumingEnumerable())
            {
                _logger.LogDetailAsync($"Queue Processor Do Work: {fileInfo.FullName} License: {_neutronLicense.CompanyCode} ").SafeFireAndForget();
                var files = new List<FileInfo>();
                if (!File.Exists(fileInfo.FullName)) continue;
                files.Add(fileInfo);
                Thread.Sleep(millisecondsTimeout: 100);

                _logger.LogDetailAsync($"TOPFileProcessor: Number of Files: {files.Count}").SafeFireAndForget();
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
          _logger.LogDetailAsync($"File Created: {e.FileInfo.FullName} ").SafeFireAndForget();
          _interfaceFileQueue.TryAdd(e.FileInfo);
        }
    }
}