using AlliedLogger;
using JsonManager;
using NeutronCore.Global;
using NeutronCore.Models;
using NeutronData.Interfaces;
using NeutronData.ModelViews;
using NeutronData.Repositories;
using NeutronEvents;
using AsyncAwaitBestPractices;

namespace NeutronLoader
{
    public class StartStopUploadManager
    {
        private readonly IJsonData _jsonData;
        private IDynamicLogger _logger;
        private IUploadProcessor _uploadProcessor;
        private readonly NeutronVariables _neutronVariables;
        private readonly NeutronLicense _neutronLicense;
        private readonly WorkstationView _workstationView;
        private IWorkstationRepository _workstationRepository;

        public StartStopUploadManager(IJsonData jsonData, NeutronVariables neutronVariables,
            NeutronLicense neutronLicense, WorkstationView workstationView)
        {
            _jsonData = jsonData;
            _neutronVariables = neutronVariables;
            _neutronLicense = neutronLicense;
            _workstationView = workstationView;
           
            Init();
        }

        private void Init()
        {
            _logger = NeutronCore.Global.Logger.SetupLogger("UploadManager");
             _workstationRepository = new WorkstationRepository(_neutronVariables);
            InitInterfaceFile();
            Mediator.GetInstance().StartStopUpload += (s, e) => StartStopAction(e.StartStop);
            Mediator.GetInstance().RunUploadOnce += async (s, e) => await _uploadProcessor.RunUploadOnce();
        }

        private void InitInterfaceFile()
        {
         _logger.LogDetailAsync($"InitInterfaceFile Company Code: {_neutronLicense.CompanyCode}").SafeFireAndForget();
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
                 _logger.LogDetailAsync($"WAG - UploadProcessorWAG").SafeFireAndForget();
                        _uploadProcessor = new UploadProcessorWAG(_neutronVariables, _logger);
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

        private void StartStopAction(string startStop)
        {
            if (startStop == "Start")
            {
                
                _logger.LogDetailAsync("Start Processing Upload Files").SafeFireAndForget();
                StartProcessingUploadFiles();
            }
            else
            {
                _logger.LogDetailAsync("Stop Processing Upload Files").SafeFireAndForget();
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
