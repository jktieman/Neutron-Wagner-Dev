using AlliedLogger;
using NeutronCore.Global;
using NeutronCore.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NeutronData.DataContexts;
using NeutronData.Interfaces;
using NeutronData.Models;
using static System.Int32;
using Timer = System.Threading.Timer;
using NeutronData.ModelViews;

namespace NeutronLoader
{
    public class UploadProcessorSfh : IUploadProcessor
    {
        private readonly NeutronLicense _neutronLicense;
        private readonly NeutronVariables _neutronVariables;
        private readonly IDynamicLogger _logger;
        private readonly WorkstationView _workstationView;
        private Timer _timer;
        private bool _uploadBusy;

        public UploadProcessorSfh(NeutronVariables neutronVariables, NeutronLicense neutronLicense,
            IDynamicLogger logger, WorkstationView workstationView)
        {
            _neutronLicense = neutronLicense;
            _neutronVariables = neutronVariables;
            _logger = logger;
            _workstationView = workstationView;
        }

        public void RunUploadOnce()
        {
            CreateHostFile();
        }

        public void StartProcessingUploadFiles()
        {
            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromSeconds(_neutronVariables.UploadDelay);
            _timer = new Timer(t => { CreateHostFile(); }, null, startTimeSpan, periodTimeSpan);
        }

        public void StopProcessingUploadFiles()
        {
            _timer.Dispose();
        }

        public void CreateHostFile()
        {
            _logger.LogDetailAsync($"Upload Processor - Creating Host File.");
            var counter = 0;
            while (_uploadBusy)
            {
                Task.Delay(200);
                ++counter;
                if (counter >= 20) return;
            }

            _uploadBusy = true;

            if (_neutronLicense.CompanyCode == "SFH")
            {
                //Remove duplicate History records before uploading
                RemoveDuplicateRecordsFromHistory();
            }

            if (string.IsNullOrWhiteSpace(_neutronVariables.ActionCodes))
            {
                MessageBox.Show(@"No Action Codes are defined.", @"Action Code Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var actionCodes = _neutronVariables.ActionCodes.Split(',').Select(Parse).ToList();
            try
            {
                using (var db = new NeutronDb())
                {
                    var recs = db.History.Where(h => !h.TransmitDateTime.HasValue && actionCodes.Contains(h.ActionCode)).ToList();
                    if (recs.Count > 0)
                    {
                        var hostFile = new HostFileSfh(_neutronLicense, _neutronVariables, _workstationView);
                        var result = hostFile.CreateHostFile(recs);
                        if (result)
                        {
                            foreach (var rec in recs)
                            {
                                rec.TransmitDateTime = DateTime.Now;
                            }
                            db.SaveChanges();
                        }
                        else
                        {
                            MessageBox.Show($"Upload Process Failed, see Log file in HostFile. Result value is: {result} ");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"Upload Process Exception, see Log file in HostFile.");
                _logger.LogDetailAsync($"Create Host File Failed: {ex.Message} {Environment.NewLine} " +
                            $"{ex.InnerException.Message} {Environment.NewLine}{ex.StackTrace}");
            }

            _uploadBusy = false;
        }

        private void RemoveDuplicateRecordsFromHistory()
        {
            try
            {
                using (var db = new NeutronDb())
                {
                    var recs = db.Database.ExecuteSqlCommand("usp_RemoveDuplicateRecordsFromHistory");
                    //if (! string.IsNullOrEmpty(recs))
                    //{
                    //     _logger.LogDetailAsync($"Remove Duplicate History Files Count: {recs} ");
                    //}

                }

            }
            catch (Exception ex)
            {
                _logger.LogDetailAsync($"Remove Duplicate History Files Error: {ex.Message} {Environment.NewLine} {ex.InnerException}");
            }
        }
    }
}
