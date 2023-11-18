using AlliedLogger;
using JsonManager;
using NeutronCore.Global;
using NeutronCore.Models;
using NeutronData.Interfaces;
using NeutronData.Models;
using NeutronData.ModelViews;
using NeutronData.Repositories;
using NeutronEvents;

namespace NeutronLoader
{
    public class StartStopUploadManager
    {
        private readonly IJsonData _jsonData;
        private readonly IDynamicLogger _logger;
        private IUploadProcessor _uploadProcessor;
        private readonly NeutronVariables _neutronVariables;
        private readonly NeutronLicense _neutronLicense;
        private readonly WorkstationView _workstationView;
        private readonly IWorkstationRepository _workstationRepository;

        public StartStopUploadManager(IJsonData jsonData, NeutronVariables neutronVariables,
            NeutronLicense neutronLicense, WorkstationView workstationView)
        {
            _jsonData = jsonData;
            _logger = NeutronCore.Global.Logger.SetupLogger("UploadManager");
            _neutronVariables = neutronVariables;
            _neutronLicense = neutronLicense;
            _workstationView = workstationView;
            _workstationRepository = new WorkstationRepository(_neutronVariables);
            InitInterfaceFile();
            Mediator.GetInstance().StartStopUpload += (s, e) => StartStopAction(e.StartStop);
            Mediator.GetInstance().RunUploadOnce += (s, e) => RunUploadOnce();
        }

        private void InitInterfaceFile()
        {
         _ = _logger.LogDetailAsync($"InitInterfaceFile Company Code: {_neutronLicense.CompanyCode}");
            switch (_neutronLicense.CompanyCode)
            {
                case "SFH":
                    {
                        _uploadProcessor = new UploadProcessorSfh(_neutronVariables, _neutronLicense, _logger
                            , _workstationView);
                        break;
                    }
                case "TOP":
                    {
                        _uploadProcessor = new UploadProcessorTop(_neutronVariables, _neutronLicense, _logger
                            , _workstationView);
                        break;
                    }
                case "MET":  
                    {
                        _uploadProcessor = new UploadProcessorMet(_neutronVariables, _neutronLicense, _logger
                            , _workstationView);
                        break;
                    }
                case "PR1":
                    {
                        _uploadProcessor = new UploadProcessorPr1(_neutronVariables, _neutronLicense, _logger
                            , _workstationView, _workstationRepository);
                        break;
                    }
                case "WAG":
                {
                 _ = _logger.LogDetailAsync($"WAG - UploadProcessorWAG");
                    _uploadProcessor = new UploadProcessorWAG(_neutronVariables, _neutronLicense, _logger, _workstationView, _workstationRepository);
                    break;
                }
                default:
                    {
                        _uploadProcessor = new UploadProcessorPr1(_neutronVariables, _neutronLicense, _logger
                            , _workstationView, _workstationRepository);
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
                
                _ = _logger.LogDetailAsync("Start Processing Upload Files");
                StartProcessingUploadFiles();
            }
            else
            {
                _ = _logger.LogDetailAsync("Stop Processing Upload Files");
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
