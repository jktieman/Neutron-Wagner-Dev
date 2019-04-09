#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using AlliedFileSystemWatcher;
using AlliedLogger;
using JsonManager;
using NeutronCore;
using NeutronCore.Global;
using NeutronCore.Models;
using NeutronEvents;
using Timer = System.Threading.Timer;

#endregion

namespace NeutronLoader
{
    public class InterfaceProcessor
    {
        private readonly BlockingCollection<FileInfo> _interfaceFileQueue = new BlockingCollection<FileInfo>();
        private static BackgroundWorker _backgroundWorker;
        private readonly DirectoryInfo _hostOrderDirectory;
        private readonly string _inputFileFilter;

        private AlliedFileWatcher _interfaceWatcher;
        readonly DynamicLogger _logger;
        private readonly NeutronVariables _neutronVariables;
        private readonly NeutronLicense _neutronLicense;
        private readonly IJsonData _jsonData;
        private string _neutronDownFileName;
        private Timer _upTimer;
        private Timer _downTimer;
        private bool _loadOrdersBusy;

        public InterfaceProcessor(NeutronVariables neutronVariables, NeutronLicense neutronLicense, IJsonData jsonData)
        {
            _neutronVariables = neutronVariables;
            _neutronLicense = neutronLicense;
            _jsonData = jsonData;

            var configFilePath = $"{Properties.Settings.Default.ConfigFilePath}";
            LoaderSettings.Init(configFilePath);
            _hostOrderDirectory = new DirectoryInfo(LoaderSettings.GetHostOrderDirectory());
            _inputFileFilter = LoaderSettings.GetHostOrderFileFilter();

            var logFileDir = LoaderSettings.GetLogFileDirectory();
            const string folderName = "Neutron Loader";
            var logActivity = LoaderSettings.EnableLogging;
            _logger = new DynamicLogger(logFileDir, folderName, logActivity);

        }

        //private void StartStopLoaderAction(string startStop)
        //{
        //    if (startStop == "Start")
        //    {
        //        StartProcessingInterfaceFiles();
        //    }
        //    else
        //    {
        //        StopProcessingInterfaceFiles();
        //    }
        //}

        public void StartProcessingInterfaceFiles()
        {


            InitBackgroundWorker();

            // _upTimer = new Timer(t => { LoadOrders(); }, null, startTimeSpan, periodTimeSpan);


            //Don't run watcher at Saint Francis Hospital
            if (_neutronLicense.CompanyCode != "SFH")
            {
                // get any existing files first
                FileInfo[] files = GetFiles();

                foreach (var file in files)
                {
                    _logger.Log($"Add File to Interface File Queue: {file.FullName} ");
                    _interfaceFileQueue.Add(file);
                }

                var startTimeSpan = TimeSpan.Zero;
                var periodTimeSpan = TimeSpan.FromMinutes(5);
                _downTimer = new Timer(t => { LoadOrders(); }, null, startTimeSpan, periodTimeSpan);

                //_interfaceWatcher = new AlliedFileWatcher(_hostOrderDirectory.ToString(), _inputFileFilter,
                //includeSubdirectories: false);
                //_interfaceWatcher.FileCreated += FileCreated;
                //_interfaceWatcher.Start();
            }
            else
            {
                var startTimeSpan = TimeSpan.Zero;
                var periodTimeSpan = TimeSpan.FromMinutes(5);
                _downTimer = new Timer(t => { LoadOrders(); }, null, startTimeSpan, periodTimeSpan);
            }

        }

        private void LoadOrders()
        {
            if (_loadOrdersBusy) return;
            _loadOrdersBusy = true;
            _logger.Log("Load Orders");
            var files = GetFiles();

            foreach (var file in files)
            {
                _logger.Log($"Add File to Interface File Queue: {file.FullName} ");
                _interfaceFileQueue.Add(file);
            }

            _loadOrdersBusy = false;
        }

        public void StopProcessingInterfaceFiles()
        {
            StopBackgroundWorker();
            _interfaceFileQueue.CompleteAdding();
            _upTimer?.Dispose();
            _interfaceWatcher?.Stop();

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
                switch (_neutronLicense.CompanyCode)
                {
                    case "TMG":
                        _logger.Log($"TMGFileProcessor: Number of Files: {files.Count}");
                        var fileProcessor = new FileProcessor(files, _neutronVariables, _neutronLicense, _jsonData,
                            _logger);
                        break;
                    case "SFH":
                        _logger.Log($"SFHFileProcessor: Number of Files: {files.Count}");
                        var pr1FileProcessor = new Pr1FileProcessor(files, _neutronLicense, _logger, _jsonData);
                        break;
                    case "AES":
                        _logger.Log($"AESFileProcessor: Number of Files: {files.Count}");
                        break;
                    case "TOP":
                        _logger.Log($"TOPFileProcessor: Number of Files: {files.Count}");
                        var topFileProcessor = new TopFileProcessor(files, _neutronVariables, _neutronLicense);
                        break;
                }
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