using SAP.Middleware.Connector;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Threading;
using System.IO;
using JsonManager;
using SAPConsole.Models;
using AlliedLogger;
using AlliedPostOffice.Concrete;
using System.Text;
using System.Net.Mail;
using SAPConsole.Extensions;

namespace SAPConsole
{
    public static class Program
    {
        public static Mutex mutex = null;
        static IJsonData _jsonData;
        static RfcDestination _rfcDest = null;
        private static string _sapServer = "WAP";
        private static int sleepTime = 60;
        // public static DirectoryInfo sapNovaSemaphore = new DirectoryInfo(@"\\WSTMS08\SapNovaSemaphore");
       // public static FileInfo novaBusyFile;
       // public static FileInfo sapBusyFile;
       // public static DynamicLogger logger;
       // public static SapNovaVariables sapNovaVariables;
       // public static SendEmail sendEmail;
        public const string AppName = "SAPConsole";

        [STAThread]
        public static void Main(string[] args)
        {
            bool createdNew;
            mutex = new Mutex(initiallyOwned: true, name: AppName, createdNew: out createdNew);
            if (!createdNew)
            {
                //app is already running!  Exiting the application
                return;
            }

            _jsonData = new JsonData(AppName);
            var sapNovaVariables = _jsonData.LoadFile<SapNovaVariables>();
            _sapServer = sapNovaVariables.SapServer;

            var novaBusyFile = new FileInfo(sapNovaVariables.NovaBusyFile);
            var sapBusyFile = new FileInfo(sapNovaVariables.SapBusyFile);


            var logger = new DynamicLogger(sapNovaVariables.LogFileFolder);


            if (!string.IsNullOrEmpty(sapNovaVariables.SapServer))
            {
                if (sapNovaVariables.SleepTime > 0)
                {
                    if (!string.IsNullOrEmpty(sapNovaVariables.Email1))
                    {
                        sleepTime = sapNovaVariables.SleepTime;
                        _sapServer = sapNovaVariables.SapServer;

                        List<string> people = new List<string>();
                        people.Add(sapNovaVariables.Email1);
                        people.Add(sapNovaVariables.Email2);
                        people.Add(sapNovaVariables.Email3);

                        var sendEmail = new SendEmail(people, logger);

                        logger.Log("SAP To Nova Record Processor.");
                        Console.WriteLine("SAP To Nova Record Processor.");
                        Console.WriteLine();
                        logger.Log($"Current SAP Server is {_sapServer}.");
                        logger.Log($"Current Sleep Time: {sleepTime} Seconds");
                        logger.Log($"Nova Busy File Location: {novaBusyFile}");
                        logger.Log($"SAP Busy File Location: {sapBusyFile}");
                        logger.Log($"Start Time: {sapNovaVariables.StartHour}:{sapNovaVariables.StartMinute}");
                        logger.Log($"Start Time: {sapNovaVariables.EndHour}:{sapNovaVariables.EndMinute}");




                        Console.WriteLine($"Current SAP Server is {_sapServer}.");
                        Console.WriteLine($"Current Sleep Time: {sleepTime} Seconds");
                        Console.WriteLine($"Nova Busy File Location: {novaBusyFile}");
                        Console.WriteLine($"SAP Busy File Location: {sapBusyFile}");
                        Console.WriteLine($"Start Time: {sapNovaVariables.StartHour}:{sapNovaVariables.StartMinute}");
                        Console.WriteLine($"Start Time: {sapNovaVariables.EndHour}:{sapNovaVariables.EndMinute}");



                       // Console.WriteLine($"Press Enter Key to begin processing.");
                       // Console.ReadLine();

                        sendEmail.StartUp();


                        while (!(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape))
                        {
                            if (RunProgramNow(sapNovaVariables))
                            {
                                if (novaBusyFile.EnsurePathExists())
                                {
                                    Console.WriteLine("Press Escape Key BETWEEN Executions to Stop.");
                                    logger.Log($"BEGIN PROCESSING...{Environment.NewLine}WSTMS08 Server is available.");


                                    if (!NovaBusy(sendEmail, novaBusyFile, logger))
                                    {
                                        sapBusyFile.Create().Dispose();
                                        Thread.Sleep(2000);

                                        ProcessRecords(sendEmail, sapNovaVariables, logger);

                                        sapBusyFile.Delete();

                                    } 
                                }
                                else
                                {
                                    logger.Log(@"WSTMS08 Server Connection Failed.");
                                    Console.WriteLine(@"WSTMS08 Server Connection Failed..");
                                    sendEmail.Message($"WSTMS08 Server Connection Failed", logger.LastLogLines());
                                }
                            }
                            Thread.Sleep(sleepTime * 1000);
                        }
                        Console.WriteLine();
                        Console.WriteLine("The screen has been paused, so you can scroll up to view the latest activity.");
                        Console.WriteLine();
                        Console.WriteLine(@"Remember, there is a log file in C:\SAPConsole\Logs");
                        Console.WriteLine();
                        Console.WriteLine("Pressing any key now, will close the program.");
                        Console.ReadLine();
                        sendEmail.ShutDown();
                    }
                    else
                    {
                        Console.WriteLine();
                        Console.WriteLine();
                        Console.WriteLine("Program failed to load.  Edit configuration file.");
                        Console.WriteLine();
                        Console.WriteLine("Email1 can not be blank.");
                        Console.WriteLine();
                        Console.WriteLine(@"C:\SAPConsole\SapNovaVariables.json");
                        Console.WriteLine();
                        Console.WriteLine("Pressing any key now, will close the program.");
                        Console.ReadLine();
                    }
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine("Program failed to load.  Edit configuration file.");
                    Console.WriteLine();
                    Console.WriteLine("Sleep Time must be greater than 0 seconds.");
                    Console.WriteLine();
                    Console.WriteLine(@"C:\SAPConsole\SapNovaVariables.json");
                    Console.WriteLine();
                    Console.WriteLine("Pressing any key now, will close the program.");
                    Console.ReadLine();
                }
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("Program failed to load.  Edit configuration file.");
                Console.WriteLine();
                Console.WriteLine("SAP Server name can not be blank.");
                Console.WriteLine();
                Console.WriteLine(@"C:\SAPConsole\SapNovaVariables.json");
                Console.WriteLine();
                Console.WriteLine("Pressing any key now, will close the program.");
                Console.ReadLine();
            }
            System.Environment.Exit(0);

        }

        private static bool RunProgramNow(SapNovaVariables sapNovaVariables)
        {
            var now = DateTime.Now;
            bool result = false;
            sleepTime = 300;

            // 5:00 AM  -  21:00 PM
            DateTime startTime = new DateTime(now.Year, now.Month, now.Day, sapNovaVariables.StartHour, sapNovaVariables.StartMinute, 0, DateTimeKind.Local);
            DateTime endTime = new DateTime(now.Year, now.Month, now.Day, sapNovaVariables.EndHour, sapNovaVariables.EndMinute, 0, DateTimeKind.Local);

            if (now > startTime && now < endTime)
            {
                result = true;
                sleepTime = sapNovaVariables.SleepTime;
            }

            return result;
        }

        private static bool NovaBusy(SendEmail sendEmail, FileInfo novaBusyFile, DynamicLogger logger)
        {
            bool result = false;
            int counter = 0;
            Console.WriteLine($"NovaBusy File Exists: {File.Exists(novaBusyFile.FullName)}");
            logger.Log($"NovaBusy File Exists: {File.Exists(novaBusyFile.FullName)}");


            while (File.Exists(novaBusyFile.FullName))
            {
                Thread.Sleep(500);
                if (!File.Exists(novaBusyFile.FullName))
                {
                    Console.WriteLine($"NovaBusy File Exists - {counter}: {novaBusyFile.Exists}");
                    logger.Log($"NovaBusy File Exists - {counter}: {novaBusyFile.Exists}");
                    result = false;
                    break;
                }
                counter += 1;
                Thread.Sleep(3000);
                if (counter == 60)
                {
                    logger.Log($"NovaBusy file is locking programs.  Counter Number: {counter}");
                    sendEmail.Message("Nova Loader has been busy too long.", new StringBuilder("See Attached Log."));

                    Console.WriteLine($"NovaBusy file is locking programs.");
                    Console.WriteLine($"Pressing Enter Key will continue SAPConsole program.");
                    Console.ReadLine();
                    result = true;
                    break;
                }

            }

            return result;
        }

        public static void ProcessRecords(SendEmail sendEmail, SapNovaVariables sapNovaVariables, DynamicLogger logger)
        {
            Console.WriteLine($"Begin Processing Records.");
            logger.Log($"Begin Processing Records.");

            if (_rfcDest == null)
            {
                try
                {
                    logger.Log($"Connecting to [ {_sapServer} ] system.");
                    Console.WriteLine($"Connecting to [ {_sapServer} ] system.");
                    var sapCfg = new SapSystemConnect(sapNovaVariables, logger);

                    RfcDestinationManager.RegisterDestinationConfiguration(sapCfg);

                    _rfcDest = RfcDestinationManager.GetDestination(sapNovaVariables.SapServer);

                    logger.Log($"SUCCESSFULLY Connected to [ {sapNovaVariables.SapServer} ] system.");
                    Console.WriteLine($"SUCCESSFULLY Connected to [ {sapNovaVariables.SapServer} ] system.");
                }
                catch (Exception ex)
                {
                    logger.Log($"Connection Failed to  {sapNovaVariables.SapServer} ./r/n  {ex.Message} {Environment.NewLine} {ex.InnerException}");
                    Console.WriteLine($"Connection Failed to  {sapNovaVariables.SapServer} ./r/n  {ex.Message} {Environment.NewLine} {ex.InnerException}");
                    sendEmail.Message($"Connection Failed to  {sapNovaVariables.SapServer}", logger.LastLogLines());
                }
            }


            if (_rfcDest != null)
            {
                logger.Log($"Receive Goods Issue - Start");
                Console.WriteLine($"Receive Goods Issue - Start");
                var sapToNovaGoodsIssue = new SapToNovaGoodsIssue(sendEmail, _jsonData, logger);
                sapToNovaGoodsIssue.Get(_rfcDest);
                Console.WriteLine($"Receive Goods Issue - Complete");
                logger.Log($"Receive Goods Issue - Complete");
                Thread.Sleep(500);

                logger.Log($"Transmit  Goods Issue - Start");
                Console.WriteLine($"Transmit  Goods Issue - Start");
                var novaToSapGoodsIssue = new NovaToSapGoodsIssue(sendEmail, logger);
                novaToSapGoodsIssue.Set(_rfcDest);
                Console.WriteLine($"Transmit  Goods Issue - Complete");
                logger.Log($"Transmit  Goods Issue - Complete");
                Thread.Sleep(500);

                logger.Log($"Receive Goods Receipts - Start");
                Console.WriteLine($"Receive Goods Receipts - Start");
                var sapToNovaGoodsReceipt = new SapToNovaGoodsReceipt(sendEmail, _jsonData, logger);
                sapToNovaGoodsReceipt.Get(_rfcDest);
                Console.WriteLine($"Receive  Goods Receipts - Complete");
                logger.Log($"Receive  Goods Receipts - Complete");
                Thread.Sleep(500);

                logger.Log($"Transmit  Goods Receipts - Start");
                Console.WriteLine($"Transmit  Goods Receipts - Start");
                var novaToSapGoodsReceipt = new NovaToSapGoodsReceipt(sendEmail, logger);
                novaToSapGoodsReceipt.Set(_rfcDest);
                Console.WriteLine($"Transmit  Goods Receipts - Complete");
                logger.Log($"Transmit  Goods Receipts - Complete");
                Thread.Sleep(500);
            }

            logger.Log($"End Processing Records.");
            Console.WriteLine($"End Processing Records.");
            Console.WriteLine();
            Console.WriteLine("Press Escape Key NOW to Stop.");
            Console.WriteLine();
            GC.Collect();

        }

    }
}
