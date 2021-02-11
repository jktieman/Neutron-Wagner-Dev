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
using Timer = System.Threading.Timer;

namespace NeutronLoader
{
    internal class InterfaceProcessorSfh : IInterfaceProcessor
    {
        private BlockingCollection<FileInfo> _interfaceFileQueue;
        private static BackgroundWorker _backgroundWorker;
        private DirectoryInfo _hostOrderDirectory;
        private string _inputFileFilter;

        // private AlliedFileWatcher _interfaceWatcher;
        private DynamicLogger _logger;
        private readonly NeutronVariables _neutronVariables;
        private readonly NeutronLicense _neutronLicense;
        private readonly IJsonData _jsonData;
        private string _neutronDownFileName;
        private Timer _downTimer;
        private bool _loadOrdersBusy;
        private int _loaderDelay;
        private const string FolderName = "Neutron Loader";
        private IFileProcessor _fileProcessor;

        public InterfaceProcessorSfh(NeutronVariables neutronVariables, NeutronLicense neutronLicense, IJsonData jsonData)
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
            _loaderDelay = _neutronVariables.LoaderDelay;
            _logger = new DynamicLogger(logFileDir, FolderName, logActivity);
            _fileProcessor = new SfhFileProcessor(_neutronVariables, _neutronLicense, _logger, _jsonData);
        }

        public void StartProcessingInterfaceFiles()
        {
            InitBackgroundWorker();
            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromMinutes(_loaderDelay);
            _downTimer = new Timer(t => { LoadOrders(); }, null, startTimeSpan, periodTimeSpan);
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
                if (_neutronLicense.CompanyCode != "SFH")
                {
                    result = _hostOrderDirectory.GetFiles(_inputFileFilter);
                }
                else
                {
                    if (!SetNeutronBusy())
                    {
                        result = _hostOrderDirectory.GetFiles(_inputFileFilter);
                    }
                    _logger.Log("Clear Neutron Down Busy Get Files Function.");
                    ClearNeutronBusy();
                }

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
                _logger.Log($"SFHFileProcessor: Number of Files: {files.Count}");
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
            _logger.Log("Call to FileCreated Function.");
            if (!SetNeutronBusy())
            {
                _logger.Log($"File Created: {e.FileInfo.FullName} ");
                _interfaceFileQueue.Add(e.FileInfo);
            }
            _logger.Log("Clear Neutron Down Busy FileCreated Function.");
            ClearNeutronBusy();
        }

        private void ClearNeutronBusy()
        {
            if (File.Exists(_neutronDownFileName))
            {
                File.Delete(_neutronDownFileName);
                _logger.Log($"ClearNeutronBusy {_neutronDownFileName} exists, deleting file.");
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

            _logger.Log($"NeutronBusyPath: {neutronBusyPath}");

            var sapBusy = Environment.GetEnvironmentVariable("SAPBUSY", EnvironmentVariableTarget.Machine);
            if (string.IsNullOrEmpty(sapBusy))
            {
                Environment.SetEnvironmentVariable("SAPBUSY", LoaderSettings.GetRootDirectory(), EnvironmentVariableTarget.Machine);
            }

            var sapBusyPath = sapBusy != null ? sapBusy.Trim() : LoaderSettings.GetRootDirectory();

            _logger.Log($"SapBusyPath: {sapBusyPath}");

            var sapDown = Environment.GetEnvironmentVariable("SAPDOWN", EnvironmentVariableTarget.Machine);
            if (string.IsNullOrEmpty(sapDown))
            {
                Environment.SetEnvironmentVariable("SAPDOWN", LoaderSettings.GetRootDirectory(), EnvironmentVariableTarget.Machine);
            }

            var sapDownPath = sapDown != null ? sapDown.Trim() : LoaderSettings.GetRootDirectory();

            _logger.Log($"SapDownPath: {sapDownPath}");

            _neutronDownFileName = neutronBusyPath + CheckForBackSlash(neutronBusyPath) + "DOWN";
            _logger.Log($"NeutronDownFileName: {_neutronDownFileName}");
            var sapDownFileName = sapBusyPath + CheckForBackSlash(sapBusyPath) + "DOWN";
            _logger.Log($"SapDownFileName: {sapDownFileName}");

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
                    _logger.Log($"{sapDownFileName} exists, waiting 1000MS.");
                }
                else
                {
                    sapFileExist = false;
                    _logger.Log($"{sapDownFileName} does not exists");
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
