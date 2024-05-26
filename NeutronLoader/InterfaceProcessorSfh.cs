using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AlliedLogger;
using AsyncAwaitBestPractices;
using JsonManager;
using NeutronCore;
using NeutronCore.Global;
using NeutronCore.Models;
using NeutronData.ModelViews;
using NeutronEvents;
using Timer = System.Timers.Timer;

namespace NeutronLoader
{
    internal class InterfaceProcessorSfh : IInterfaceProcessor
    {
        private BlockingCollection<FileInfo> _interfaceFileQueue;
        private static BackgroundWorker _backgroundWorker;
        private DirectoryInfo _hostOrderDirectory;
        private string _inputFileFilter;
        private readonly IDynamicLogger _logger;
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
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        
        public InterfaceProcessorSfh(NeutronVariables neutronVariables, NeutronLicense neutronLicense,
            IJsonData jsonData, WorkstationView workstationView)
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
            _loaderDelay = _neutronVariables.LoaderDelay;
            _fileProcessor = new SfhFileProcessor(_neutronVariables, _neutronLicense, _logger, _jsonData, _workstationView);
            InitBackgroundWorker();
        }

        public void ErrorAlert(string err)
        {
            Mediator.GetInstance().OnLoaderError(this, err);
        }
        public async Task StartProcessingInterfaceFiles()
        {
            //var startTimeSpan = TimeSpan.Zero;
            //var periodTimeSpan = TimeSpan.FromSeconds(_loaderDelay);
            //_timer = new Timer(t => { LoadOrders(); }, null, startTimeSpan, periodTimeSpan);
            _timer = new Timer(_neutronVariables.LoaderDelay * 1000);
            //_timer.Elapsed += async (sender, e) => await LoadOrders();
            _timer.Elapsed += async (sender, e) =>
            {
                if (_semaphore.CurrentCount == 0)
                {
                    return;
                }
                await _semaphore.WaitAsync();
                try
                {
                    await LoadOrders();
                }
                finally
                {
                    _semaphore.Release();
                }
            };
            _timer.Start();
            await Task.Delay(10);
        }

        private async Task LoadOrders()
        {
            if (_loadOrdersBusy) return;
           // _timer?.Stop();
            _loadOrdersBusy = true;
         _logger.LogDetailAsync("Order Processor - Load Orders").SafeFireAndForget();

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

            _loadOrdersBusy = false;
           // _timer?.Start();
        }

        public void StopProcessingInterfaceFiles()
        {
            StopBackgroundWorker();
            _interfaceFileQueue.CompleteAdding();
            _timer?.Dispose();
        }

        public async Task RunLoaderOnce() => await LoadOrders();
        

        public FileInfo[] GetFiles()
        {
         _logger.LogDetailAsync("Call to Get Files Function.").SafeFireAndForget();
            var result = new FileInfo[] { };
            try
            {
                if (!SetNeutronBusy())
                {
                    result = _hostOrderDirectory.GetFiles(_inputFileFilter);
                }
             _logger.LogDetailAsync("Clear Neutron Down Busy Get Files Function.").SafeFireAndForget();
                ClearNeutronBusy();
            }
            catch (Exception ex)
            {
             _logger.LogDetailAsync(
                    $"Get Files Error.  {Environment.NewLine} {ex.Message} {Environment.NewLine} {ex.InnerException?.Message} {Environment.NewLine}  {ex.InnerException?.InnerException?.Message}").SafeFireAndForget();
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
             _logger.LogDetailAsync($"SFHFileProcessor: Number of Files: {files.Count}").SafeFireAndForget();
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
             _logger.LogDetailAsync($"ClearNeutronBusy {_neutronDownFileName} exists, deleting file.").SafeFireAndForget();
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

         _logger.LogDetailAsync($"NeutronBusyPath: {neutronBusyPath}").SafeFireAndForget();

            var sapBusy = Environment.GetEnvironmentVariable("SAPBUSY", EnvironmentVariableTarget.Machine);
            if (string.IsNullOrEmpty(sapBusy))
            {
                Environment.SetEnvironmentVariable("SAPBUSY", LoaderSettings.GetRootDirectory(), EnvironmentVariableTarget.Machine);
            }

            var sapBusyPath = sapBusy != null ? sapBusy.Trim() : LoaderSettings.GetRootDirectory();

         _logger.LogDetailAsync($"SapBusyPath: {sapBusyPath}").SafeFireAndForget();

            var sapDown = Environment.GetEnvironmentVariable("SAPDOWN", EnvironmentVariableTarget.Machine);
            if (string.IsNullOrEmpty(sapDown))
            {
                Environment.SetEnvironmentVariable("SAPDOWN", LoaderSettings.GetRootDirectory(), EnvironmentVariableTarget.Machine);
            }

            var sapDownPath = sapDown != null ? sapDown.Trim() : LoaderSettings.GetRootDirectory();

         _logger.LogDetailAsync($"SapDownPath: {sapDownPath}").SafeFireAndForget();

            _neutronDownFileName = neutronBusyPath + CheckForBackSlash(neutronBusyPath) + "DOWN";
         _logger.LogDetailAsync($"NeutronDownFileName: {_neutronDownFileName}").SafeFireAndForget();
            var sapDownFileName = sapBusyPath + CheckForBackSlash(sapBusyPath) + "DOWN";
         _logger.LogDetailAsync($"SapDownFileName: {sapDownFileName}").SafeFireAndForget();

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
                 _logger.LogDetailAsync($"{sapDownFileName} exists, waiting 1000MS.").SafeFireAndForget();
                }
                else
                {
                    sapFileExist = false;
                 _logger.LogDetailAsync($"{sapDownFileName} does not exists").SafeFireAndForget();
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
