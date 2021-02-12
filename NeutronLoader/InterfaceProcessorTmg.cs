#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using AlliedLogger;
using JsonManager;
using NeutronCore;
using NeutronCore.Global;
using NeutronCore.Models;
using Timer = System.Threading.Timer;

#endregion

namespace NeutronLoader
{
    public class InterfaceProcessorTmg : IInterfaceProcessor
    {
        private BlockingCollection<FileInfo> _interfaceFileQueue;
        private static BackgroundWorker _backgroundWorker;
        private DirectoryInfo _hostOrderDirectory;
        private string _inputFileFilter;
        private DynamicLogger _logger;
        private readonly NeutronVariables _neutronVariables;
        private readonly NeutronLicense _neutronLicense;
        private readonly IJsonData _jsonData;
        private Timer _downTimer;
        private bool _loadOrdersBusy;
        private const string FolderName = "Neutron Loader";
        private IFileProcessor _fileProcessor;

        public InterfaceProcessorTmg(NeutronVariables neutronVariables, NeutronLicense neutronLicense, IJsonData jsonData)
        {
            _neutronVariables = neutronVariables;
            _neutronLicense = neutronLicense;
            _jsonData = jsonData;
            Initialize();
        }

        private void Initialize()
        {
            _interfaceFileQueue = new BlockingCollection<FileInfo>();
            LoaderSettings.Init();
            _hostOrderDirectory = new DirectoryInfo(LoaderSettings.GetHostOrderDirectory());
            _inputFileFilter = LoaderSettings.GetHostOrderFileFilter();
            var logFileDir = LoaderSettings.GetLogFileDirectory();
            var logActivity = LoaderSettings.EnableLogging;
            _logger = new DynamicLogger(logFileDir, FolderName, logActivity);
            _fileProcessor = new Pr1FileProcessor(_neutronVariables, _neutronLicense, _logger, _jsonData);
        }

        public void StartProcessingInterfaceFiles()
        {
            InitBackgroundWorker();
            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromMinutes(5);
            _downTimer = new Timer(t => { LoadOrders(); }, null, startTimeSpan, periodTimeSpan);

            ////Don't run watcher at Saint Francis Hospital
            //if (_neutronLicense.CompanyCode != "SFH")
            //{
            //    // get any existing files first
            //    FileInfo[] files = GetFiles();

            //    foreach (var file in files)
            //    {
            //        _logger.Log($"Add File to Interface File Queue: {file.FullName} ");
            //        _interfaceFileQueue.Add(file);
            //    }

            //    var startTimeSpan = TimeSpan.Zero;
            //    var periodTimeSpan = TimeSpan.FromMinutes(5);
            //    _downTimer = new Timer(t => { LoadOrders(); }, null, startTimeSpan, periodTimeSpan);

            //    //_interfaceWatcher = new AlliedFileWatcher(_hostOrderDirectory.ToString(), _inputFileFilter,
            //    //includeSubdirectories: false);
            //    //_interfaceWatcher.FileCreated += FileCreated;
            //    //_interfaceWatcher.Start();
            //}
            //else
            //{
            //  var startTimeSpan = TimeSpan.Zero;
            //  var periodTimeSpan = TimeSpan.FromMinutes(5);
            //  _downTimer = new Timer(t => { LoadOrders(); }, null, startTimeSpan, periodTimeSpan);
            //}

        }

        private void LoadOrders()
        {
            if (_loadOrdersBusy) return;
            _loadOrdersBusy = true;
            _logger.Log("Load Orders");


            try
            {
                var files = GetFiles();

                foreach (var file in files)
                {
                    _logger.Log($"Add File to Interface File Queue: {file.FullName} ");

                    _interfaceFileQueue.TryAdd(file);

                    Thread.Sleep(100);
                }
            }
            catch (ObjectDisposedException oex)
            {
                _logger.Log($"LoadOrders Object Disposed Exception {Environment.NewLine} {oex.Message}");
            }
            catch (InvalidOperationException iex)
            {
                _logger.Log($"LoadOrders Invalid Operation Exception {Environment.NewLine} {iex.Message}");
            }
            catch (Exception ex)
            {
                _logger.Log($"LoadOrders Exception {Environment.NewLine} {ex.Message}");
            }

            _loadOrdersBusy = false;
        }

        public void StopProcessingInterfaceFiles()
        {
            StopBackgroundWorker();
            _interfaceFileQueue.CompleteAdding();
            // _upTimer?.Dispose();
            //  _interfaceWatcher?.Stop();

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
            _logger.Log("Call to Get Files Function.");
            var result = new FileInfo[] { };
            try
            {
                result = _hostOrderDirectory.GetFiles(_inputFileFilter);
            }
            catch (Exception ex)
            {
                _logger.Log(
                    $"Get Files Error.  {Environment.NewLine} {ex.Message} {Environment.NewLine} {ex.InnerException?.Message} {Environment.NewLine}  {ex.InnerException?.InnerException?.Message}");
            }

            return result;
        }

        private void StopBackgroundWorker()
        {
            _backgroundWorker.CancelAsync();
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
            _logger.Log("Queue Processor Do Work");
            if (_backgroundWorker.CancellationPending)
            {
                _logger.Log($"BackgroundWorker Cancel.");
                e.Cancel = true;
                return;
            }

            Thread.Sleep(millisecondsTimeout: 100);
            foreach (var fileInfo in _interfaceFileQueue.GetConsumingEnumerable())
            {
                _logger.Log($"Queue Processor Do Work: {fileInfo.FullName} License: {_neutronLicense.CompanyCode} ");
                var files = new List<FileInfo>();
                if (!File.Exists(fileInfo.FullName)) continue;
                files.Add(fileInfo);
                Thread.Sleep(millisecondsTimeout: 100);

                _logger.Log($"TMGFileProcessor: Number of Files: {files.Count}");
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
    }
}