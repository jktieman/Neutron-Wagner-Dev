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
using Timer = System.Threading.Timer;

namespace NeutronLoader
{
    internal class InterfaceProcessorSfh : IInterfaceProcessor
    {
        private BlockingCollection<FileInfo> _interfaceFileQueue;
        private static BackgroundWorker _backgroundWorker;
        private DirectoryInfo _hostOrderDirectory;
        private string _inputFileFilter;
        private IDynamicLogger _logger;
        private readonly NeutronVariables _neutronVariables;
        private readonly NeutronLicense _neutronLicense;
        private readonly IJsonData _jsonData;
        private readonly WorkstationView _workstationView;
        private string _neutronDownFileName;
        private Timer _timer;
        private bool _loadOrdersBusy;
        private int _loaderDelay;
        private const string FolderName = "Neutron Loader";
        private IFileProcessor _fileProcessor;

        public InterfaceProcessorSfh(NeutronVariables neutronVariables, NeutronLicense neutronLicense,
            IJsonData jsonData, WorkstationView workstationView, IDynamicLogger logger)
        {
          
            _logger = logger;
            _neutronVariables = neutronVariables;
            _neutronLicense = neutronLicense;
            _jsonData = jsonData;
            _workstationView = workstationView;
            Initialize();
        }

        private void Initialize()
        {
            _interfaceFileQueue = new BlockingCollection<FileInfo>();
            LoaderSettings.Init();
            _hostOrderDirectory = new DirectoryInfo(LoaderSettings.GetHostOrderDirectory());
            _inputFileFilter = LoaderSettings.GetHostOrderFileFilter();
           // var logFileDir = LoaderSettings.GetLogFileDirectory();
           // var logActivity = LoaderSettings.EnableLogging;
            _loaderDelay = _neutronVariables.LoaderDelay;
           // _logger = new DynamicLogger(logFileDir, FolderName, logActivity);
            _fileProcessor = new SfhFileProcessor(_neutronVariables, _neutronLicense, _logger, _jsonData, _workstationView);
            InitBackgroundWorker();
        }

        public void ErrorAlert(string err)
        {
            Mediator.GetInstance().OnLoaderError(this, err);
        }
        public void StartProcessingInterfaceFiles()
        {
            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromSeconds(_loaderDelay);
            _timer = new Timer(t => { LoadOrders(); }, null, startTimeSpan, periodTimeSpan);
        }

        private void LoadOrders()
        {
            if (_loadOrdersBusy) return;
            _loadOrdersBusy = true;
            _logger.LogDetailAsync("Order Processor - Load Orders");

            try
            {
                var files = GetFiles();

                foreach (var file in files)
                {
                    _logger.LogDetailAsync($"Add File to Interface File Queue: {file.FullName} ");

                    _interfaceFileQueue.TryAdd(file);

                    Thread.Sleep(100);
                }
            }
            catch (ObjectDisposedException oex)
            {
                _logger.LogDetailAsync($"LoadOrders Object Disposed Exception {Environment.NewLine} {oex.Message}");
            }
            catch (InvalidOperationException iex)
            {
                _logger.LogDetailAsync($"LoadOrders Invalid Operation Exception {Environment.NewLine} {iex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"LoadOrders Exception {Environment.NewLine} {ex.Message}");
            }

            _loadOrdersBusy = false;
        }

        public void StopProcessingInterfaceFiles()
        {
            StopBackgroundWorker();
            _interfaceFileQueue.CompleteAdding();
            _timer?.Dispose();
        }

        public void RunLoaderOnce()
        {
            LoadOrders();
        }

        public FileInfo[] GetFiles()
        {
            _logger.LogDetailAsync("Call to Get Files Function.");
            var result = new FileInfo[] { };
            try
            {
                if (!SetNeutronBusy())
                {
                    result = _hostOrderDirectory.GetFiles(_inputFileFilter);
                }
                _logger.LogDetailAsync("Clear Neutron Down Busy Get Files Function.");
                ClearNeutronBusy();
            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync(
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
            _logger.LogDetailAsync("Queue Processor Do Work");
            if (_backgroundWorker.CancellationPending)
            {
                _logger.LogDetailAsync($"BackgroundWorker Cancel.");
                e.Cancel = true;
                return;
            }

            Thread.Sleep(millisecondsTimeout: 100);
            foreach (var fileInfo in _interfaceFileQueue.GetConsumingEnumerable())
            {
                _logger.LogDetailAsync($"Queue Processor Do Work: {fileInfo.FullName} License: {_neutronLicense.CompanyCode} ");
                var files = new List<FileInfo>();
                if (!File.Exists(fileInfo.FullName)) continue;
                files.Add(fileInfo);
                Thread.Sleep(millisecondsTimeout: 100);
                _logger.LogDetailAsync($"SFHFileProcessor: Number of Files: {files.Count}");
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

        private void ClearNeutronBusy()
        {
            if (File.Exists(_neutronDownFileName))
            {
                File.Delete(_neutronDownFileName);
                _logger.LogDetailAsync($"ClearNeutronBusy {_neutronDownFileName} exists, deleting file.");
            }
        }

        private bool SetNeutronBusy()
        {
            var counter = 0;
            var sapFileExist = true;

            var neutronBusy = Environment.GetEnvironmentVariable("NEUTRONBUSY", EnvironmentVariableTarget.Machine);

            if (string.IsNullOrEmpty(neutronBusy))
            {
                Environment.SetEnvironmentVariable("NEUTRONBUSY", LoaderSettings.GetRootDirectory(), EnvironmentVariableTarget.Machine);
            }

            var neutronBusyPath = neutronBusy != null ? neutronBusy.Trim() : LoaderSettings.GetRootDirectory();

            _logger.LogDetailAsync($"NeutronBusyPath: {neutronBusyPath}");

            var sapBusy = Environment.GetEnvironmentVariable("SAPBUSY", EnvironmentVariableTarget.Machine);
            if (string.IsNullOrEmpty(sapBusy))
            {
                Environment.SetEnvironmentVariable("SAPBUSY", LoaderSettings.GetRootDirectory(), EnvironmentVariableTarget.Machine);
            }

            var sapBusyPath = sapBusy != null ? sapBusy.Trim() : LoaderSettings.GetRootDirectory();

            _logger.LogDetailAsync($"SapBusyPath: {sapBusyPath}");

            var sapDown = Environment.GetEnvironmentVariable("SAPDOWN", EnvironmentVariableTarget.Machine);
            if (string.IsNullOrEmpty(sapDown))
            {
                Environment.SetEnvironmentVariable("SAPDOWN", LoaderSettings.GetRootDirectory(), EnvironmentVariableTarget.Machine);
            }

            var sapDownPath = sapDown != null ? sapDown.Trim() : LoaderSettings.GetRootDirectory();

            _logger.LogDetailAsync($"SapDownPath: {sapDownPath}");

            _neutronDownFileName = neutronBusyPath + CheckForBackSlash(neutronBusyPath) + "DOWN";
            _logger.LogDetailAsync($"NeutronDownFileName: {_neutronDownFileName}");
            var sapDownFileName = sapBusyPath + CheckForBackSlash(sapBusyPath) + "DOWN";
            _logger.LogDetailAsync($"SapDownFileName: {sapDownFileName}");

            while (counter <= 10)
            {
                using (var down = new StreamWriter(_neutronDownFileName, append: true))
                {
                    down.WriteLine("Neutron");
                }

                if (File.Exists(sapDownFileName))
                {
                    sapFileExist = true;
                    File.Delete(_neutronDownFileName);
                    counter++;
                    Thread.Sleep(100);
                    _logger.LogDetailAsync($"{sapDownFileName} exists, waiting 1000MS.");
                }
                else
                {
                    sapFileExist = false;
                    _logger.LogDetailAsync($"{sapDownFileName} does not exists");
                    break;
                }
            }

            return sapFileExist;
        }


        private string CheckForBackSlash(string neutronBusyPath)
        {
            return neutronBusyPath.EndsWith(@"\") ? string.Empty : @"\";
        }
    }
}
