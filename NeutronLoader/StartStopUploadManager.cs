using AlliedLogger;
using JsonManager;
using NeutronCore.Global;
using NeutronCore.Models;
using NeutronEvents;

namespace NeutronLoader
{
    public class StartStopUploadManager
    {
        private readonly IJsonData _jsonData;
        private readonly DynamicLogger _logger;
        private IUploadProcessor _uploadProcessor;
        private readonly NeutronVariables _neutronVariables;
        private readonly NeutronLicense _neutronLicense;
       // private static Timer _upTimer;
       // private static bool _processingUpload;

        public StartStopUploadManager(IJsonData jsonData, DynamicLogger logger, NeutronVariables neutronVariables, NeutronLicense neutronLicense)
        {
            _jsonData = jsonData;
            _logger = logger;
            _neutronVariables = neutronVariables;
            _neutronLicense = neutronLicense;
            Mediator.GetInstance().StartStopUpload += (s, e) => StartStopAction(e.StartStop);
        }

        private void StartStopAction(string startStop)
        {
            if (startStop == "Start")
            {
                _logger.Log("Start Processing Upload Files");
                StartProcessingUploadFiles();
            }
            else
            {
                _logger.Log("Stop Processing Upload Files");
                StopProcessingUploadFiles();
            }
        }

        private void StartProcessingUploadFiles()
        {
            switch (_neutronLicense.CompanyCode)
            {
                case "SFH":
                    {
                        //_uploadProcessor = new InterfaceProcessorSfh(_neutronVariables, _neutronLicense, _jsonData);
                        //_uploadProcessor.StartProcessingInterfaceFiles();

                        //var startTimeSpan = TimeSpan.Zero;
                        //var periodTimeSpan = TimeSpan.FromMinutes(5);
                        //_upTimer = new Timer(t => { CreateHostUploadFile(); }, null, startTimeSpan, periodTimeSpan);
                        break;
                    }
                case "TOP":
                    {
                    //    _uploadProcessor = new InterfaceProcessorTop(_neutronVariables, _neutronLicense, _jsonData);
                    //    _uploadProcessor.StartProcessingInterfaceFiles();
                        break;
                    }
                case "TMG":
                    {
                        //_uploadProcessor = new InterfaceProcessorTmg(_neutronVariables, _neutronLicense, _jsonData);
                        //_uploadProcessor.StartProcessingInterfaceFiles();
                        break;
                    }
                case "PR1":
                    {
                        //var startTimeSpan = TimeSpan.Zero;
                        //var periodTimeSpan = TimeSpan.FromMinutes(5);
                        //_upTimer = new Timer(t => { CreateHostUploadFile(); }, null, startTimeSpan, periodTimeSpan);
                        _uploadProcessor = new UploadProcessorPr1(_neutronLicense, _neutronVariables, _logger);
                        _uploadProcessor.StartProcessingUploadFiles();
                        //_uploadProcessor.CreateHostFile();
                        break;
                    }
            }

        }

        private void StopProcessingUploadFiles()
        {
            _uploadProcessor?.StopProcessingUploadFiles();
        }

        //public void CreateHostUploadFile()
        //{
        //    if (_neutronLicense.CompanyCode == "SFH")
        //    {
        //        //Remove duplicate History records before uploading
        //        RemoveDuplicateRecordsFromHistory();
        //    }

        //    if (_processingUpload) return;
        //    _processingUpload = true;
        //    var uploadProcessor = new UploadProcessor(_neutronLicense, _neutronVariables, _logger);
        //    uploadProcessor.CreateHostFile();
        //    _processingUpload = false;
        //}

        //public void RemoveDuplicateRecordsFromHistory()
        //{
        //    try
        //    {
        //        using (var db = new NeutronDb())
        //        {
        //            var recs = db.Database.ExecuteSqlCommand("usp_RemoveDuplicateRecordsFromHistory");
        //            //if (! string.IsNullOrEmpty(recs))
        //            //{
        //            //     _logger.Log($"Remove Duplicate History Files Count: {recs} ");
        //            //}

        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Log($"Remove Duplicate History Files Error: {ex.Message} {Environment.NewLine} {ex.InnerException}");
        //    }
        //}
    }
}
