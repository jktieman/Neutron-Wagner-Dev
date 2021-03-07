using AlliedLogger;
using NeutronCore.Global;
using NeutronCore.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NeutronData.DataContexts;
using NeutronData.Models;
using static System.Int32;
using Timer = System.Threading.Timer;

namespace NeutronLoader
{
    public class UploadProcessorPr1 : IUploadProcessor
    {
        private readonly NeutronLicense _neutronLicense;
        private readonly NeutronVariables _neutronVariables;
        private readonly DynamicLogger _logger;
        private readonly Station _rackStation;
        private Timer _timer;
        private bool _uploadBusy;

        public UploadProcessorPr1(NeutronVariables neutronVariables, NeutronLicense neutronLicense,
            DynamicLogger logger, Station rackStation)
        {
            _neutronLicense = neutronLicense;
            _neutronVariables = neutronVariables;
            _logger = logger;
            _rackStation = rackStation;
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
            var counter = 0;
            while (_uploadBusy)
            {
                Task.Delay(200);
                ++counter;
                if (counter >= 20) return;
            }

            _uploadBusy = true;
            var actionCodes = _neutronVariables.ActionCodes.Split(',').Select(Parse).ToList();
            try
            {
                using (var db = new NeutronDb())
                {
                    var recs = db.History.Where(h => !h.TransmitDateTime.HasValue && actionCodes.Contains(h.ActionCode)).ToList();
                    if (recs.Count > 0)
                    {
                        var hostFile = new HostFilePr1(_neutronLicense, _neutronVariables);
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
                            MessageBox.Show(@"Upload Process Failed, see Log file in HostFile.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"Upload Process Failed, see Log file in HostFile.");
                _logger.Log($"Create Host File Failed: {ex.Message} {Environment.NewLine} {ex.InnerException}");
            }

            _uploadBusy = false;
        }
    }
}
