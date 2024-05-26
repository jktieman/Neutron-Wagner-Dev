
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
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
    public class InterfaceProcessor : IInterfaceProcessor
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
        private Timer _timer;
        private bool _loadOrdersBusy;
        private const string FolderName = "Neutron Loader";
        private IFileProcessor _fileProcessor;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        public InterfaceProcessor(NeutronVariables neutronVariables, NeutronLicense neutronLicense, IJsonData jsonData
            , WorkstationView workstationView)
        {

            _neutronVariables = neutronVariables;
            _neutronLicense = neutronLicense;
            _jsonData = jsonData;
            _workstationView = workstationView;
            Init();  
        }

        private void Init()
        {
            _logger = NeutronCore.Global.Logger.SetupLogger("InterfaceProcessor");
            _interfaceFileQueue = new BlockingCollection<FileInfo>();
            LoaderSettings.Init();
            _hostOrderDirectory = new DirectoryInfo(LoaderSettings.GetHostOrderDirectory());
            _inputFileFilter = LoaderSettings.GetHostOrderFileFilter();
            _fileProcessor = new Pr1FileProcessor(_neutronVariables, _neutronLicense, _jsonData, _workstationView);
        }

        public async Task StartProcessingInterfaceFiles()
        {
            InitBackgroundWorker();
            //var startTimeSpan = TimeSpan.Zero;
            //var periodTimeSpan = TimeSpan.FromSeconds(_neutronVariables.LoaderDelay);
            //_timer = new Timer(t => { LoadOrders(); }, null, startTimeSpan, periodTimeSpan);
            _timer = new Timer(_neutronVariables.LoaderDelay * 1000);
            // _timer.Elapsed += async (sender, e) => await LoadOrders();
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
            //_timer?.Stop();
            
            _loadOrdersBusy = true;
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

            _loadOrdersBusy = false;
           // _timer?.Start();
        }

        public void ErrorAlert(string err)
        {
            Mediator.GetInstance().OnLoaderError(this, err);
        }

        public void StopProcessingInterfaceFiles()
        {
            StopBackgroundWorker();
            _interfaceFileQueue.CompleteAdding();
        }

        public async Task RunLoaderOnce()
        {
            var files = GetFiles().ToList();
            await Task.Delay(10);
            if (files.Count > 0)
            {
                _logger.LogDetailAsync($"FileProcessor: Number of Files: {files.Count}").SafeFireAndForget();
                _fileProcessor.LoadFiles(files);
            }
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
                _logger.LogDetailAsync(
                        $"Queue Processor Do Work: {fileInfo.FullName} License: {_neutronLicense.CompanyCode} ")
                    .SafeFireAndForget(); 
                var files = new List<FileInfo>();
                if (!File.Exists(fileInfo.FullName)) continue;
                files.Add(fileInfo);
                Thread.Sleep(millisecondsTimeout: 100);
                _logger.LogDetailAsync($"FileProcessor: Number of Files: {files.Count}").SafeFireAndForget();
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
    }
}
