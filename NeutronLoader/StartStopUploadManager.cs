using AlliedLogger;
using JsonManager;
using NeutronCore.Global;
using NeutronCore.Models;
using NeutronData.Models;
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
        private readonly Station _rackStation;

        public StartStopUploadManager(IJsonData jsonData, DynamicLogger logger, NeutronVariables neutronVariables,
            NeutronLicense neutronLicense, Station rackStation)
        {
            _jsonData = jsonData;
            _logger = logger;
            _neutronVariables = neutronVariables;
            _neutronLicense = neutronLicense;
            _rackStation = rackStation;
            InitInterfaceFile();
            Mediator.GetInstance().StartStopUpload += (s, e) => StartStopAction(e.StartStop);
            Mediator.GetInstance().RunUploadOnce += (s, e) => RunUploadOnce();
        }

        private void InitInterfaceFile()
        {
            switch (_neutronLicense.CompanyCode)
            {
                case "SFH":
                    {
                        _uploadProcessor = new UploadProcessorSfh(_neutronVariables, _neutronLicense, _logger, _rackStation);
                        break;
                    }
                case "TOP":
                    {
                        _uploadProcessor = new UploadProcessorTop(_neutronVariables, _neutronLicense, _logger, _rackStation);
                        break;
                    }
                case "MET":  // using Topura Upload Process
                    {
                        _uploadProcessor = new UploadProcessorMet(_neutronVariables, _neutronLicense, _logger, _rackStation);
                        break;
                    }
                case "PR1":
                    {
                        _uploadProcessor = new UploadProcessorPr1(_neutronVariables, _neutronLicense, _logger, _rackStation);
                        break;
                    }
                default:
                    {
                        _uploadProcessor = new UploadProcessorPr1(_neutronVariables, _neutronLicense, _logger, _rackStation);
                        break;
                    }
            }
        }

        private void RunUploadOnce()
        {
            _uploadProcessor.RunUploadOnce();
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
            _uploadProcessor.StartProcessingUploadFiles();
        }

        private void StopProcessingUploadFiles()
        {
            _uploadProcessor.StopProcessingUploadFiles();
        }
    }
}
