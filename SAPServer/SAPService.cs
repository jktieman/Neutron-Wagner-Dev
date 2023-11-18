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
using System.Windows.Forms;
using SAPServer.Extensions;
using NeutronEvents;

namespace SAPServer
{
    // ReSharper disable once InconsistentNaming
    public class SAPService : ISapService
    {
        public Mutex Mutex;
        private readonly IJsonData _jsonData;
        private RfcDestination _rfcDestination;
        private string _sapServer = "WAQ";
        private readonly IDynamicLogger _logger;
        private int _sleepTime = 60;
        public const string AppName = "SAPConsole";
        private SapVariables _sapVariables;

        public SAPService(IJsonData jsonData)
        {
            _jsonData = jsonData;
            // _sendEmail = sendEmail;
            _logger = NeutronCore.Global.Logger.SetupLogger("SAPService");
            Init();
        }

        public void Init()
        {
            try
            {
                _sapVariables = _jsonData.LoadFile<SapVariables>();
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync($"Error loading SAP Variables File.  {ex.Message}");
                ErrorAlert(ex.Message);
                throw new Exception($"Error loading SAP Variables File.  {ex.Message}");
            }

            _sapServer = _sapVariables.SapServer;

            var neutronBusyFile = new FileInfo(_sapVariables.NeutronBusyFile);
            var sapBusyFile = new FileInfo(_sapVariables.SapBusyFile);


            try
            {
                if (!string.IsNullOrEmpty(_sapVariables.SapServer))
                {
                    if (_sapVariables.SleepTime > 0)
                    {
                        if (!string.IsNullOrEmpty(_sapVariables.Email1))
                        {
                            _sleepTime = _sapVariables.SleepTime;
                            _sapServer = _sapVariables.SapServer;

                            var people = new List<string>
                        {
                            _sapVariables.Email1,
                            _sapVariables.Email2,
                            _sapVariables.Email3
                        };

                            _ = _logger.LogDetailAsync("SAP To Neutron Record Processor.");
                            _ = _logger.LogDetailAsync($"Current SAP Server is {_sapServer}.");
                            _ = _logger.LogDetailAsync($"Current Sleep Time: {_sleepTime} Seconds");
                            _ = _logger.LogDetailAsync($"Neutron Busy File Location: {neutronBusyFile}");
                            _ = _logger.LogDetailAsync($"SAP Busy File Location: {sapBusyFile}");
                            _ = _logger.LogDetailAsync($"Start Time: {_sapVariables.StartHour}:{_sapVariables.StartMinute}");
                            _ = _logger.LogDetailAsync($"Start Time: {_sapVariables.EndHour}:{_sapVariables.EndMinute}");

                            //_sendEmail.StartUp();

                            if (RunProgramNow(_sapVariables))
                            {
                                if (neutronBusyFile.EnsurePathExists())
                                {
                                    Console.WriteLine("Press Escape Key BETWEEN Executions to Stop.");
                                    _ = _logger.LogDetailAsync($"BEGIN PROCESSING...{Environment.NewLine}WSTMS08 Server is available.");


                                    if (!NeutronBusy(neutronBusyFile))
                                    {
                                        sapBusyFile.Create().Dispose();
                                        Thread.Sleep(2000);

                                       // MessageBox.Show($"Skipping SAP Processing until we get correct drivers.");
                                        ProcessRecords(_sapVariables);

                                        sapBusyFile.Delete();

                                    }
                                }
                                else
                                {
                                    _ = _logger.LogDetailAsync(@"WSTMS08 Server Connection Failed.");

                                    //_sendEmail.Message($"WSTMS08 Server Connection Failed", _logger.LastLogLines());
                                }
                            }

                            //_sendEmail.ShutDown();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync($"Error loading SAP Services.  {ex.Message}");
                ErrorAlert(ex.Message);
                throw new Exception($"Error loading SAP Services.  {ex.Message}");
            }

            //Environment.Exit(0);

        }
        public void ErrorAlert(string err)
        {
            Mediator.GetInstance().OnLoaderError(this, err);
        }

        private bool RunProgramNow(SapVariables sapVariables)
        {
            var now = DateTime.Now;
            bool result = false;
            _sleepTime = 300;

            // 5:00 AM  -  21:00 PM
            DateTime startTime = new DateTime(now.Year, now.Month, now.Day, sapVariables.StartHour,
                sapVariables.StartMinute, 0, DateTimeKind.Local);
            DateTime endTime = new DateTime(now.Year, now.Month, now.Day, sapVariables.EndHour,
                sapVariables.EndMinute, 0, DateTimeKind.Local);

            if (now > startTime && now < endTime)
            {
                result = true;
                _sleepTime = sapVariables.SleepTime;
            }

            return result;
        }

        private bool NeutronBusy(FileInfo neutronBusyFile)
        {
            var result = false;
            var counter = 0;
            _ = _logger.LogDetailAsync($"Neutron Busy File Exists: {File.Exists(neutronBusyFile.FullName)}");

            while (File.Exists(neutronBusyFile.FullName))
            {
                Thread.Sleep(500);
                if (!File.Exists(neutronBusyFile.FullName))
                {
                    _ = _logger.LogDetailAsync($"Neutron Busy File Exists - {counter}: {neutronBusyFile.Exists}");
                    break;
                }

                counter += 1;
                Thread.Sleep(3000);
                if (counter != 60) continue;
                _ = _logger.LogDetailAsync($"Neutron Busy file is locking programs.  Counter Number: {counter}");
                //_sendEmail.Message("Neutron Loader has been busy too long.", new StringBuilder("See Attached Log."));
                result = true;
                break;
            }
            return result;
        }

        public void ProcessRecords(SapVariables sapVariables)
        {
            _ = _logger.LogDetailAsync($"Begin Processing Records.");

            if (_rfcDestination == null)
            {
                try
                {
                    _ = _logger.LogDetailAsync($"Connecting to [ {_sapServer} ] system.");

                    // SapSystemConnect implements IDestinationConfiguration
                    var sapCfg = new SapSystemConnect(sapVariables, _logger);

                    // Register the custom destination configuration
                    RfcDestinationManager.RegisterDestinationConfiguration(sapCfg);

                    // Get the destination
                    _rfcDestination = RfcDestinationManager.GetDestination(sapVariables.SapServer);

                    _ = _logger.LogDetailAsync($"SUCCESSFULLY Connected to [ {sapVariables.SapServer} ] system.");
                }
                catch (Exception ex)
                {
                    _ = _logger.LogDetailAsync(
                           $"Connection Failed to  {sapVariables.SapServer} ./r/n  {ex.Message} {Environment.NewLine} {ex.InnerException}");
                    //_sendEmail.Message($"Connection Failed to  {sapVariables.SapServer}", _logger.LastLogLines());
                }
            }


            if (_rfcDestination != null)
            {
                _ = _logger.LogDetailAsync($"Receive Goods Issue - Start");
                var sapToNeutronGoodsIssue = new SapToNeutronGoodsIssue(_jsonData, _logger);
                sapToNeutronGoodsIssue.Get(_rfcDestination);
                _ = _logger.LogDetailAsync($"Receive Goods Issue - Complete");
                Thread.Sleep(500);
            
            //Testing
                _ = _logger.LogDetailAsync($"Testing: All processes stopped early UnComment from here down when finished.");


                //_ = _logger.LogDetailAsync($"Transmit  Goods Issue - Start");
                //var neutronToSapGoodsIssue = new NeutronToSapGoodsIssue(_logger);
                //neutronToSapGoodsIssue.Set(_rfcDestination);
                //_ = _logger.LogDetailAsync($"Transmit  Goods Issue - Complete");
                //Thread.Sleep(500);

                //_ = _logger.LogDetailAsync($"Receive Goods Receipts - Start");
                //var sapToNeutronGoodsReceipt = new SapToNeutronGoodsReceipt(_jsonData, _logger);
                //sapToNeutronGoodsReceipt.Get(_rfcDestination);
                //_ = _logger.LogDetailAsync($"Receive  Goods Receipts - Complete");
                //Thread.Sleep(500);

                //_ = _logger.LogDetailAsync($"Transmit  Goods Receipts - Start");
                //var neutronToSapGoodsReceipt = new NeutronToSapGoodsReceipt(_logger);
                //neutronToSapGoodsReceipt.Set(_rfcDestination);
                //_ = _logger.LogDetailAsync($"Transmit  Goods Receipts - Complete");
                //Thread.Sleep(500);
            }

            _ = _logger.LogDetailAsync($"End Processing Records.");
            
            //GC.Collect();
        }
    }
}
