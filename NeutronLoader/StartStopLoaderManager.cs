using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AlliedLogger;
using JsonManager;
using NeutronCore.Global;
using NeutronCore.Models;
using NeutronData.DataContexts;
using NeutronEvents;

namespace NeutronLoader
{
    public class StartStopLoaderManager
    {
        private readonly IJsonData _jsonData;
        private readonly DynamicLogger _logger;
        private IInterfaceProcessor _interfaceProcessor;
        private readonly NeutronVariables _neutronVariables;
        private readonly NeutronLicense _neutronLicense;
        private static Timer _upTimer;
        private static bool _processingUpload;

        public StartStopLoaderManager(IJsonData jsonData, DynamicLogger logger)
        {
            _jsonData = jsonData;
            _logger = logger;
            _neutronVariables = jsonData.LoadFile<NeutronVariables>();
            _neutronLicense = jsonData.LoadFile<NeutronLicense>();
            Mediator.GetInstance().StartStopLoader += (s, e) => StartStopLoaderAction(e.StartStop);
        }

        private void StartStopLoaderAction(string startStop)
        {
            if (startStop == "Start")
            {
                _logger.Log("Start Processing Interface Files");
                StartProcessingInterfaceFiles();
            }
            else
            {
                _logger.Log("Stop Processing Interface Files");
                StopProcessingInterfaceFiles();
            }
        }

        private void StartProcessingInterfaceFiles()
        {
            switch (_neutronLicense.CompanyCode)
            {
                case "SFH":
                    {
                        _interfaceProcessor = new InterfaceProcessorSfh(_neutronVariables, _neutronLicense, _jsonData);
                        _interfaceProcessor.StartProcessingInterfaceFiles();

                        var startTimeSpan = TimeSpan.Zero;
                        var periodTimeSpan = TimeSpan.FromMinutes(5);
                        _upTimer = new Timer(t => { CreateHostUploadFile(); }, null, startTimeSpan, periodTimeSpan);
                        break;
                    }
                case "TOP":
                    {
                        _interfaceProcessor = new InterfaceProcessorTop(_neutronVariables, _neutronLicense, _jsonData);
                        _interfaceProcessor.StartProcessingInterfaceFiles();
                        break;
                    }
                case "TMG":
                    {
                        _interfaceProcessor = new InterfaceProcessorTmg(_neutronVariables, _neutronLicense, _jsonData);
                        _interfaceProcessor.StartProcessingInterfaceFiles();
                        break;
                    }
            }

        }

        private void StopProcessingInterfaceFiles()
        {
            _interfaceProcessor?.StopProcessingInterfaceFiles();
            _upTimer?.Dispose();
        }

        public void CreateHostUploadFile()
        {
            if (_neutronLicense.CompanyCode == "SFH")
            {
                //Remove duplicate History records before uploading
                RemoveDuplicateRecordsFromHistory();
            }

            if (_processingUpload) return;
            _processingUpload = true;
            var uploadProcessor = new UploadProcessor(_neutronLicense, _neutronVariables, _logger);
            uploadProcessor.CreateHostFile();
            _processingUpload = false;
        }

        public void RemoveDuplicateRecordsFromHistory()
        {
            try
            {
                using (var db = new NeutronDb())
                {
                    var recs = db.Database.ExecuteSqlCommand("usp_RemoveDuplicateRecordsFromHistory");
                    //if (! string.IsNullOrEmpty(recs))
                    //{
                    //     _logger.Log($"Remove Duplicate History Files Count: {recs} ");
                    //}

                }

            }
            catch (Exception ex)
            {
                _logger.Log($"Remove Duplicate History Files Error: {ex.Message} {Environment.NewLine} {ex.InnerException}");
            }
        }
    }
}
