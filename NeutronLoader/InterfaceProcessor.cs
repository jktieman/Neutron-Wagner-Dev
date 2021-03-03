
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using AlliedLogger;
using JsonManager;
using NeutronCore;
using NeutronCore.Global;
using NeutronCore.Models;
using Timer = System.Threading.Timer;


namespace NeutronLoader
{
    public class InterfaceProcessor : IInterfaceProcessor
    {
        private BlockingCollection<FileInfo> _interfaceFileQueue;
        private static BackgroundWorker _backgroundWorker;
        private DirectoryInfo _hostOrderDirectory;
        private string _inputFileFilter;
        private DynamicLogger _logger;
        private readonly NeutronVariables _neutronVariables;
        private readonly NeutronLicense _neutronLicense;
        private readonly IJsonData _jsonData;
        private Timer _timer;
        private bool _loadOrdersBusy;
        private const string FolderName = "Neutron Loader";
        private IFileProcessor _fileProcessor;

        public InterfaceProcessor(NeutronVariables neutronVariables, NeutronLicense neutronLicense, IJsonData jsonData)
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
            var periodTimeSpan = TimeSpan.FromSeconds(_neutronVariables.LoaderDelay);
            _timer = new Timer(t => { LoadOrders(); }, null, startTimeSpan, periodTimeSpan);
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
        }

        public void RunLoaderOnce()
        {
            var files = GetFiles().ToList();
            if (files.Count > 0)
            {
                _logger.Log($"FileProcessor: Number of Files: {files.Count}");
                _fileProcessor.LoadFiles(files);
            }
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
            _backgroundWorker?.CancelAsync();
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
                _logger.Log($"FileProcessor: Number of Files: {files.Count}");
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

        private string CheckForBackSlash(string neutronBusyPath)
        {
            return neutronBusyPath.EndsWith(@"\") ? string.Empty : @"\";
        }

        public void RunLoaderContinuously()
        {
            
        }
    }
}
