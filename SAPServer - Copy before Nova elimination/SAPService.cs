using AlliedLogger;
using AlliedPostOffice;
using JsonManager;
using SAP.Middleware.Connector;
using SAPServer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SAPServer.Extensions;

namespace SAPServer
{
    // ReSharper disable once InconsistentNaming
    public class SAPService : ISAPService
    {
        public Mutex Mutex;
        private IJsonData _jsonData;
        private RfcDestination _rfcDest;
        private string _sapServer = "WAQ";
        private IDynamicLogger _logger;
        private int _sleepTime = 60;

        // public static DirectoryInfo sapNovaSemaphore = new DirectoryInfo(@"\\WSTMS08\SapNovaSemaphore");
        // public static FileInfo novaBusyFile;
        // public static FileInfo sapBusyFile;
        // public static DynamicLogger logger;
        // public static SapNovaVariables sapNovaVariables;
       // private ISendEmail _sendEmail;
        public const string AppName = "SAPConsole";

        public SAPService(IJsonData jsonData)
        {
            _jsonData = jsonData;
            // _sendEmail = sendEmail;
            _logger = NeutronCore.Global.Logger.SetupLogger("SAPService");
            Init();
        }

        public void Init()
        {
            Mutex = new Mutex(initiallyOwned: true, name: AppName, createdNew: out var createdNew);
            if (!createdNew)
            {
                //app is already running!  Exiting the application
                return;
            }

            //_jsonData = new JsonData(AppName);
            var sapNovaVariables = _jsonData.LoadFile<SapNovaVariables>();
            _sapServer = sapNovaVariables.SapServer;

            var novaBusyFile = new FileInfo(sapNovaVariables.NovaBusyFile);
            var sapBusyFile = new FileInfo(sapNovaVariables.SapBusyFile);


           // var logger = new DynamicLogger(sapNovaVariables.LogFileFolder);


            if (!string.IsNullOrEmpty(sapNovaVariables.SapServer))
            {
                if (sapNovaVariables.SleepTime > 0)
                {
                    if (!string.IsNullOrEmpty(sapNovaVariables.Email1))
                    {
                        _sleepTime = sapNovaVariables.SleepTime;
                        _sapServer = sapNovaVariables.SapServer;

                        var people = new List<string>
                        {
                            sapNovaVariables.Email1,
                            sapNovaVariables.Email2,
                            sapNovaVariables.Email3
                        };

                        _logger.Log("SAP To Neutron Record Processor.");
                        _logger.LogDetailAsync($"Current SAP Server is {_sapServer}.");
                        _logger.LogDetailAsync($"Current Sleep Time: {_sleepTime} Seconds");
                        _logger.LogDetailAsync($"Neutron Busy File Location: {novaBusyFile}");
                        _logger.LogDetailAsync($"SAP Busy File Location: {sapBusyFile}");
                        _logger.LogDetailAsync($"Start Time: {sapNovaVariables.StartHour}:{sapNovaVariables.StartMinute}");
                        _logger.LogDetailAsync($"Start Time: {sapNovaVariables.EndHour}:{sapNovaVariables.EndMinute}");

                        //_sendEmail.StartUp();

                        if (RunProgramNow(sapNovaVariables))
                        {
                            if (novaBusyFile.EnsurePathExists())
                            {
                                Console.WriteLine("Press Escape Key BETWEEN Executions to Stop.");
                                _logger.LogDetailAsync($"BEGIN PROCESSING...{Environment.NewLine}WSTMS08 Server is available.");


                                if (!NovaBusy(novaBusyFile))
                                {
                                    sapBusyFile.Create().Dispose();
                                    Thread.Sleep(2000);

                                    ProcessRecords(sapNovaVariables);

                                    sapBusyFile.Delete();

                                }
                            }
                            else
                            {
                                _logger.LogDetailAsync(@"WSTMS08 Server Connection Failed.");

                                //_sendEmail.Message($"WSTMS08 Server Connection Failed", _logger.LastLogLines());
                            }
                        }

                        //_sendEmail.ShutDown();
                    }
                }
            }

            //Environment.Exit(0);

        }

        private bool RunProgramNow(SapNovaVariables sapNovaVariables)
        {
            var now = DateTime.Now;
            bool result = false;
            _sleepTime = 300;

            // 5:00 AM  -  21:00 PM
            DateTime startTime = new DateTime(now.Year, now.Month, now.Day, sapNovaVariables.StartHour,
                sapNovaVariables.StartMinute, 0, DateTimeKind.Local);
            DateTime endTime = new DateTime(now.Year, now.Month, now.Day, sapNovaVariables.EndHour,
                sapNovaVariables.EndMinute, 0, DateTimeKind.Local);

            if (now > startTime && now < endTime)
            {
                result = true;
                _sleepTime = sapNovaVariables.SleepTime;
            }

            return result;
        }

        private bool NovaBusy(FileInfo novaBusyFile)
        {
            var result = false;
            var counter = 0;
            Console.WriteLine($"NovaBusy File Exists: {File.Exists(novaBusyFile.FullName)}");
            _logger.LogDetailAsync($"NovaBusy File Exists: {File.Exists(novaBusyFile.FullName)}");


            while (File.Exists(novaBusyFile.FullName))
            {
                Thread.Sleep(500);
                if (!File.Exists(novaBusyFile.FullName))
                {
                    Console.WriteLine($"NovaBusy File Exists - {counter}: {novaBusyFile.Exists}");
                    _logger.LogDetailAsync($"NovaBusy File Exists - {counter}: {novaBusyFile.Exists}");
                    break;
                }

                counter += 1;
                Thread.Sleep(3000);
                if (counter != 60) continue;
                _logger.LogDetailAsync($"NovaBusy file is locking programs.  Counter Number: {counter}");
                //_sendEmail.Message("Neutron Loader has been busy too long.", new StringBuilder("See Attached Log."));

                Console.WriteLine($"NovaBusy file is locking programs.");
                Console.WriteLine($"Pressing Enter Key will continue SAPConsole program.");
                Console.ReadLine();
                result = true;
                break;

            }

            return result;
        }

        public void ProcessRecords(SapNovaVariables sapNovaVariables)
        {
            Console.WriteLine($"Begin Processing Records.");
            _logger.LogDetailAsync($"Begin Processing Records.");

            if (_rfcDest == null)
            {
                try
                {
                    _logger.LogDetailAsync($"Connecting to [ {_sapServer} ] system.");
                    Console.WriteLine($"Connecting to [ {_sapServer} ] system.");
                    var sapCfg = new SapSystemConnect(sapNovaVariables, _logger);

                    RfcDestinationManager.RegisterDestinationConfiguration(sapCfg);

                    _rfcDest = RfcDestinationManager.GetDestination(sapNovaVariables.SapServer);

                    _logger.LogDetailAsync($"SUCCESSFULLY Connected to [ {sapNovaVariables.SapServer} ] system.");
                    Console.WriteLine($"SUCCESSFULLY Connected to [ {sapNovaVariables.SapServer} ] system.");
                }
                catch (Exception ex)
                {
                    _logger.LogDetailAsync(
                        $"Connection Failed to  {sapNovaVariables.SapServer} ./r/n  {ex.Message} {Environment.NewLine} {ex.InnerException}");
                    Console.WriteLine(
                        $"Connection Failed to  {sapNovaVariables.SapServer} ./r/n  {ex.Message} {Environment.NewLine} {ex.InnerException}");
                    //_sendEmail.Message($"Connection Failed to  {sapNovaVariables.SapServer}", _logger.LastLogLines());
                }
            }


            if (_rfcDest != null)
            {
                _logger.LogDetailAsync($"Receive Goods Issue - Start");
                Console.WriteLine($"Receive Goods Issue - Start");
               // var sapToNovaGoodsIssue = new SapToNovaGoodsIssue(_sendEmail, _jsonData, _logger);
                var sapToNovaGoodsIssue = new SapToNovaGoodsIssue(_jsonData, _logger);
                sapToNovaGoodsIssue.Get(_rfcDest);
                Console.WriteLine($"Receive Goods Issue - Complete");
                _logger.LogDetailAsync($"Receive Goods Issue - Complete");
                Thread.Sleep(500);

                _logger.LogDetailAsync($"Transmit  Goods Issue - Start");
                Console.WriteLine($"Transmit  Goods Issue - Start");
                //var novaToSapGoodsIssue = new NovaToSapGoodsIssue(_sendEmail, _logger);
                var novaToSapGoodsIssue = new NovaToSapGoodsIssue(_logger);
                novaToSapGoodsIssue.Set(_rfcDest);
                Console.WriteLine($"Transmit  Goods Issue - Complete");
                _logger.LogDetailAsync($"Transmit  Goods Issue - Complete");
                Thread.Sleep(500);

                _logger.LogDetailAsync($"Receive Goods Receipts - Start");
                Console.WriteLine($"Receive Goods Receipts - Start");
               // var sapToNovaGoodsReceipt = new SapToNovaGoodsReceipt(_sendEmail, _jsonData, _logger);
                var sapToNovaGoodsReceipt = new SapToNovaGoodsReceipt(_jsonData, _logger);
                sapToNovaGoodsReceipt.Get(_rfcDest);
                Console.WriteLine($"Receive  Goods Receipts - Complete");
                _logger.LogDetailAsync($"Receive  Goods Receipts - Complete");
                Thread.Sleep(500);

                _logger.LogDetailAsync($"Transmit  Goods Receipts - Start");
                Console.WriteLine($"Transmit  Goods Receipts - Start");
                //var novaToSapGoodsReceipt = new NovaToSapGoodsReceipt(_sendEmail, _logger);
                var novaToSapGoodsReceipt = new NovaToSapGoodsReceipt( _logger);
                novaToSapGoodsReceipt.Set(_rfcDest);
                Console.WriteLine($"Transmit  Goods Receipts - Complete");
                _logger.LogDetailAsync($"Transmit  Goods Receipts - Complete");
                Thread.Sleep(500);
            }

            _logger.LogDetailAsync($"End Processing Records.");
            Console.WriteLine($"End Processing Records.");
            Console.WriteLine();
            Console.WriteLine("Press Escape Key NOW to Stop.");
            Console.WriteLine();
            GC.Collect();

        }
    }
}
