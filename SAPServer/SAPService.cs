using AlliedLogger;
using JsonManager;
using SAP.Middleware.Connector;
using SAPServer.Models;
using System;
using System.IO;
using System.Threading;
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
        private SapVariables _sapVariables;
        private FileInfo _neutronBusyFile;
        private FileInfo _sapBusyFile;

        public SAPService(IJsonData jsonData)
        {
            _jsonData = jsonData;
            _logger = NeutronCore.Global.Logger.SetupLogger("SAPService");
        }

        public void Init()
        {
            try
            {
                _sapVariables = _jsonData.LoadFile<SapVariables>();
                //Mediator.GetInstance().OnSendEmailMessage(this, "Neutron/SAP Loader Started");
            }
            catch (Exception ex)
            {
                Mediator.GetInstance().OnSendEmailMessage(this, $"Error loading SAP Variables File.  {ex.Message}");
                _ = _logger.LogDetailAsync($"Error loading SAP Variables File.  {ex.Message}");
                ErrorAlert(ex.Message);
                //throw new Exception($"Error loading SAP Variables File.  {ex.Message}");
            }

            _sleepTime = _sapVariables.SleepTime;
            _sapServer = _sapVariables.SapServer;

            _neutronBusyFile = new FileInfo(_sapVariables.NeutronBusyFile);
            _sapBusyFile = new FileInfo(_sapVariables.SapBusyFile);

            if (_sapVariables.SleepTime <= 0) return;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("SAP To Neutron Record Processor.");
            sb.AppendLine($"Current SAP Server is {_sapServer}.");
            sb.AppendLine($"Current Sleep Time: {_sleepTime} Seconds");
            sb.AppendLine($"Neutron Busy File Location: {_neutronBusyFile}");
            sb.AppendLine($"SAP Busy File Location: {_sapBusyFile}");
            sb.AppendLine(
                $"Start Time: {_sapVariables.StartHour}:{_sapVariables.StartMinute}");
            sb.AppendLine(
                $"Stop Time: {_sapVariables.EndHour}:{_sapVariables.EndMinute}");

            Mediator.GetInstance().OnSendEmailMessage(this, $"Neutron/SAP Loader Information.{Environment.NewLine}{sb}");
        }

        public void Run()
        {
            try
            {
                if (string.IsNullOrEmpty(_sapVariables.SapServer)) return;

                if (!RunProgramNow(_sapVariables)) return;
                if (_neutronBusyFile.EnsurePathExists())
                {
                    _ = _logger.LogDetailAsync(
                        $"BEGIN PROCESSING...{Environment.NewLine}Neutron Busy File is available.");

                    if (NeutronBusy(_neutronBusyFile)) return;
                    _sapBusyFile.Create().Dispose();
                    Thread.Sleep(2000);

                    ProcessRecords(_sapVariables);

                    _sapBusyFile.Delete();
                }
                else
                {
                    _ = _logger.LogDetailAsync("Neutron Busy File does not exist.");
                    ErrorAlert("Neutron Busy File does not exist");
                    Mediator.GetInstance().OnSendEmailMessage(this, "Neutron Busy File does not exist.");
                }
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync($"Error loading SAP Services.  {ex.Message}");
                Mediator.GetInstance().OnSendEmailMessage(this, $"Error loading SAP Services.{Environment.NewLine}{ex.Message}");
                ErrorAlert($"Error loading SAP Services.{Environment.NewLine}{ex.Message}");
            }
        }

        private void ErrorAlert(string err)
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
                Mediator.GetInstance().OnSendEmailMessage(this, "Neutron Loader has been busy for three minutes.");
                result = true;
                break;
            }
            return result;
        }

        /// <summary>
        /// Processes the SAP records based on the provided SAP variables.
        /// </summary>
        /// <param name="sapVariables">The SAP variables used to connect and interact with the SAP server.</param>
        /// <remarks>
        /// This method performs the following operations in order:
        /// 1. Logs the start of the processing.
        /// 2. Establishes a connection with the SAP server.
        /// 3. If the connection is successful, it performs the following operations:
        ///    - Receives and processes Goods Issue from SAP to Nova.
        ///    - Transmits and processes Goods Issue from Nova to SAP.
        ///    - Receives and processes Goods Receipts from SAP to Nova.
        ///    - Transmits and processes Goods Receipts from Nova to SAP.
        /// 4. Logs the end of the processing.
        /// </remarks>
        private void ProcessRecords(SapVariables sapVariables)
        {
            _ = _logger.LogDetailAsync("Begin Processing Records.");

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
                           $"Connection Failed to  {sapVariables.SapServer}{Environment.NewLine} {ex.Message} {Environment.NewLine} {ex.InnerException}");
                    Mediator.GetInstance().OnSendEmailMessage(this, $"Connection Failed to: {sapVariables.SapServer}");
                }
            }


            if (_rfcDestination != null)
            {
                _ = _logger.LogDetailAsync("Receive Goods Issue - Start");
                var sapToNovaGoodsIssue = new SapToNovaGoodsIssue(_jsonData, _logger);
                sapToNovaGoodsIssue.Get(_rfcDestination);
                _ = _logger.LogDetailAsync("Receive Goods Issue - Complete");
                Thread.Sleep(500);

                _ = _logger.LogDetailAsync("Transmit  Goods Issue - Start");
                var novaToSapGoodsIssue = new NovaToSapGoodsIssue(_logger);
                novaToSapGoodsIssue.Set(_rfcDestination);
                _ = _logger.LogDetailAsync("Transmit  Goods Issue - Complete");
                Thread.Sleep(500);

                _ = _logger.LogDetailAsync("Receive Goods Receipts - Start");
                var sapToNovaGoodsReceipt = new SapToNovaGoodsReceipt(_jsonData, _logger);
                sapToNovaGoodsReceipt.Get(_rfcDestination);
                _ = _logger.LogDetailAsync("Receive  Goods Receipts - Complete");
                Thread.Sleep(500);

                _ = _logger.LogDetailAsync("Transmit  Goods Receipts - Start");
                var novaToSapGoodsReceipt = new NovaToSapGoodsReceipt(_logger);
                novaToSapGoodsReceipt.Set(_rfcDestination);
                _ = _logger.LogDetailAsync("Transmit  Goods Receipts - Complete");
                Thread.Sleep(500);
            }

            _ = _logger.LogDetailAsync("End Processing Records.");

            //GC.Collect();
        }
    }
}
