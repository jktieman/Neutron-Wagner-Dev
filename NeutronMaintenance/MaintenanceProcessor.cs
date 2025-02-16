using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO;
using System.Threading;
using AlliedLogger;
using NeutronCore;
using NeutronCore.Global;
using NeutronData.DataContexts;
using NeutronData.Models;
using NeutronData.Repositories;
using JsonManager;
using NeutronCore.Models;


namespace NeutronMaintenance
{
    public class MaintenanceProcessor
    {
        private readonly BlockingCollection<FileInfo> interfaceFileQueue = new BlockingCollection<FileInfo>();
        private static BackgroundWorker _interfaceFileQueueProcessor;
        private readonly GenericRepository<Inventory> _repoInventory;
        private readonly GenericRepository<ItemDefinition> _repoItemDefinition;
        private readonly GenericRepository<Location> _repoLocation;
        
        //private readonly HistoryManager historyManager = new HistoryManager();
       // private readonly AlliedFileWatcher interfaceWatcher;
        
       private readonly IDynamicLogger _logger;
        string _configFilePath;
        readonly bool _usePr1Processor = false;
        readonly IJsonData _jsonData;
        readonly NeutronVariables _neutronVariables;
        readonly NeutronLicense _neutronLicense;

        public MaintenanceProcessor(IJsonData jsonData, string configFilePath, Func<NeutronDb> contextFactory)
        {
            if (contextFactory == null) throw new ArgumentNullException(nameof(contextFactory));
            _jsonData = jsonData;
            _configFilePath = configFilePath;
            _neutronVariables = jsonData.LoadFile<NeutronVariables>();
            _neutronLicense = jsonData.LoadFile<NeutronLicense>();
            _usePr1Processor = _neutronVariables.UsePr1Processor;
            LoaderSettings.Init();
            _repoInventory = new GenericRepository<Inventory>(contextFactory);
            _repoItemDefinition = new GenericRepository<ItemDefinition>(contextFactory);
            _repoLocation = new GenericRepository<Location>(contextFactory);


            _logger = NeutronCore.Global.Logger.SetupLogger("MaintenanceProcessor");
        }



        public void StartProcessingFiles()
        {
            InitBackgroundWorker();

            //  FileInfo[] files = GetFiles();
            //if (usePr1Processor)
            //{
            //    pr1FileProcessor = new Pr1FileProcessor(files);
            //}
            //else
            //{
            //    fileProcessor = new FileProcessor(files);
            //}
         _ = _logger.LogDetailAsync($"Background Worker Company Code: {_neutronLicense.CompanyCode}");
            switch (_neutronLicense.CompanyCode)
            {
                case "TMG":
                    // fileProcessor = new FileProcessor(files);
                    break;
                case "SFH":
                    //  pr1FileProcessor = new Pr1FileProcessor(files);
                    break;
                case "AES":
                    break;
                case "TOP":
                    break;
            }

            //interfaceWatcher = new AlliedFileWatcher(hostOrderDirectory.ToString(), inputFileFilter, includeSubdirectories: false);
            //interfaceWatcher.FileCreated += FileCreated;
            //interfaceWatcher.Start();
        }

        public void StopProcessingInterfaceFiles()
        {
                StopBackgroundWorker();
              //  interfaceWatcher.Stop();
        }

        //public FileInfo[] GetFiles()
        //{
        //    var result = new FileInfo[] { };
        //    try
        //    {
        //        result = hostOrderDirectory.GetFiles(inputFileFilter);
        //    }
        //    catch (Exception ex)
        //    {
        //    _ = _logger.LogDetailAsync($"Get Files Error.  \r\n {ex.Message} \r\n {ex.InnerException.Message} \r\n  {ex.InnerException.InnerException.Message}");
        //    }
        //    return result;
        //}

        private void StopBackgroundWorker()
        {
            _interfaceFileQueueProcessor.CancelAsync();
        }

        private void InitBackgroundWorker()
        {
            _interfaceFileQueueProcessor = new BackgroundWorker
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = true
            };
            _interfaceFileQueueProcessor.DoWork += InterfaceFileQueueProcessorDoWork;
            _interfaceFileQueueProcessor.RunWorkerCompleted += InterfaceFileQueueProcessorRunWorkerCompleted;
        }

        private void InterfaceFileQueueProcessorDoWork(object sender, DoWorkEventArgs e)
        {
            if (_interfaceFileQueueProcessor.CancellationPending)
            {
                e.Cancel = true;
                return;
            }
            var i = 0;
            var files = new FileInfo[] { };
            Thread.Sleep(millisecondsTimeout: 1000);
            foreach (var fileInfo in interfaceFileQueue.GetConsumingEnumerable())
            {
                var filename = fileInfo.FullName;
                if (File.Exists(filename))
                {
                    files[i] = fileInfo;
                    i += 1;
                    Thread.Sleep(millisecondsTimeout: 100);

                }
            }
         _ = _logger.LogDetailAsync($"Form Pick Company Code: {_neutronLicense.CompanyCode}");
            switch (_neutronLicense.CompanyCode)
            {
                case "TMG":
                    // fileProcessor = new FileProcessor(files);
                    break;
                case "SFH":
                    //pr1FileProcessor = new Pr1FileProcessor(files);
                    break;
                case "AES":
                    break;
                case "TOP":
                    break;
            }

            foreach (var file in files)
            {
                ArchiveFile.Archive(file, _logger);
            }
        }

        private void InterfaceFileQueueProcessorRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {

            }

            else
            {
                var result = e.Result;

            }
        }

        //internal void FileCreated(object sender, FileInfoArgs e)
        //{
        //    var prevFiles = new FileInfo[] { };
        //    var currentFiles = new FileInfo[] { };
        //    try
        //    {
        //        prevFiles = GetFiles();
        //        Thread.Sleep(millisecondsTimeout: 1000);
        //        currentFiles = GetFiles();
        //        while (prevFiles.Count() != currentFiles.Count())
        //        {
        //            prevFiles = currentFiles;
        //            Thread.Sleep(millisecondsTimeout: 1000);
        //            currentFiles = GetFiles();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //     _ = _logger.LogDetailAsync("File Created Error.  \r\n" + ex.Message + "\r\n" + ex.InnerException.Message + "\r\n" + ex.InnerException.InnerException.Message);
        //    }

        //    switch (neutronLicense.CompanyCode)
        //    {
        //        case "TMG":
        //            fileProcessor = new FileProcessor(currentFiles);
        //            break;
        //        case "SFH":
        //            pr1FileProcessor = new Pr1FileProcessor(currentFiles);
        //            break;
        //        case "AES":
        //            break;
        //        case "TOP":
        //            break;

        //        default:
        //            break;
        //    }


        //if (usePr1Processor)
        //{
        //    pr1FileProcessor = new Pr1FileProcessor(currentFiles);
        //}
        //else
        //{
        //    fileProcessor = new FileProcessor(currentFiles);
        //}
        //}

        private void ShowFileLockMessages(OperationResult fileLockFailure)
        {
            foreach (var msg in fileLockFailure.MessageList)
            {
             _ = _logger.LogDetailAsync("Lock Message " + msg);
            }
        }
    }
}
