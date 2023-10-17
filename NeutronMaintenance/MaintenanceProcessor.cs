using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO;
using System.Threading;
using AlliedFileSystemWatcher;
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
    public partial class MaintenanceProcessor
    {
        private readonly BlockingCollection<FileInfo> interfaceFileQueue = new BlockingCollection<FileInfo>();
        private static BackgroundWorker interfaceFileQueueProcessor;
        private readonly GenericRepository<Inventory> repoInventory = new GenericRepository<Inventory>(new NeutronDb());
        private readonly GenericRepository<ItemDefinition> repoItemDefinition = new GenericRepository<ItemDefinition>(new NeutronDb());
        private readonly GenericRepository<Location> repoLocation = new GenericRepository<Location>(new NeutronDb());
        //private readonly HistoryManager historyManager = new HistoryManager();
        private readonly AlliedFileWatcher interfaceWatcher;
        private IDynamicLogger _logger;
        string configFilePath;
        readonly bool usePr1Processor = false;
        readonly IJsonData jsonData;
        readonly NeutronVariables neutronVariables;
        readonly NeutronLicense neutronLicense;

        public MaintenanceProcessor(IJsonData jsonData, string configFilePath)
        {
            this.jsonData = jsonData;
            this.configFilePath = configFilePath;
            neutronVariables = jsonData.LoadFile<NeutronVariables>();
            neutronLicense = jsonData.LoadFile<NeutronLicense>();
            usePr1Processor = neutronVariables.UsePr1Processor;
            LoaderSettings.Init();

            var logFileDir = LoaderSettings.GetLogFileDirectory();
            var folderName = @"Neutron Maintenance";
            var logActivity = LoaderSettings.EnableLogging;
            _logger = new DynamicLogger(logFileDir, folderName, logActivity);

            //usePr1Processor = Convert.ToBoolean(LoaderSettings.UsePr1Format);
            //if (usePr1Processor)
            //{
            //    var masterProcessor = new MasterMaintenanceProcessor();
            //    masterProcessor.ProcessMasterMaintenanceFiles();

            //}


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
            _logger.LogDetailAsync($"Background Worker Company Code: {neutronLicense.CompanyCode}");
            switch (neutronLicense.CompanyCode)
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
                interfaceWatcher.Stop();
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
        //       _logger.LogDetailAsync($"Get Files Error.  \r\n {ex.Message} \r\n {ex.InnerException.Message} \r\n  {ex.InnerException.InnerException.Message}");
        //    }
        //    return result;
        //}

        private void StopBackgroundWorker()
        {
            interfaceFileQueueProcessor.CancelAsync();
        }

        private void InitBackgroundWorker()
        {
            interfaceFileQueueProcessor = new BackgroundWorker
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = true
            };
            interfaceFileQueueProcessor.DoWork += InterfaceFileQueueProcessorDoWork;
            interfaceFileQueueProcessor.RunWorkerCompleted += InterfaceFileQueueProcessorRunWorkerCompleted;
        }

        private void InterfaceFileQueueProcessorDoWork(object sender, DoWorkEventArgs e)
        {
            if (interfaceFileQueueProcessor.CancellationPending)
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
            _logger.LogDetailAsync($"Form Pick Company Code: {neutronLicense.CompanyCode}");
            switch (neutronLicense.CompanyCode)
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
        //        _logger.LogDetailAsync("File Created Error.  \r\n" + ex.Message + "\r\n" + ex.InnerException.Message + "\r\n" + ex.InnerException.InnerException.Message);
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
                _logger.LogDetailAsync("Lock Message " + msg);
            }
        }
    }
}
