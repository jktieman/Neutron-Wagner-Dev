using AlliedLogger;
using NeutronCore.Global;
using NeutronCore.Models;
using NeutronData.Models;
using NeutronData.ModelViews;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Threading.Timer;
using static System.Int32;
using NeutronData.DataContexts;

namespace NeutronLoader
{
    public class UploadProcessorMet : IUploadProcessor
    {
        private readonly NeutronLicense _neutronLicense;
        private readonly NeutronVariables _neutronVariables;
        private readonly DynamicLogger _logger;
        private readonly Station _rackStation;
        private Timer _timer;
        private Timer _replenTimer;

        public bool UploadBusy;
        public bool ReplenBusy;

        public UploadProcessorMet(NeutronVariables neutronVariables, NeutronLicense neutronLicense,
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
            CreateReplenFile();
        }

        public void StartProcessingUploadFiles()
        {
            _logger.Log($"Start Processing Upload Files");
            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromSeconds(_neutronVariables.UploadDelay);
            _timer = new Timer(t => { CreateHostFile(); }, null, startTimeSpan, periodTimeSpan);

            _logger.Log($"Start Processing Skip Replenishment");
             startTimeSpan = TimeSpan.Zero;
             periodTimeSpan = TimeSpan.FromSeconds(_neutronVariables.UploadDelay);
            _replenTimer = new Timer(t => { CreateReplenFile(); }, null, startTimeSpan, periodTimeSpan);

        }

        public void StopProcessingUploadFiles()
        {
            _timer?.Dispose();
            _replenTimer?.Dispose();
        }

        public void CreateHostFile()
        {
            var counter = 0;
            while (UploadBusy)
            {
                Thread.Sleep(1000);
                counter += 1;
                if (counter > 60)
                {
                    UploadBusy = false;
                    break;
                }
            }

            UploadBusy = true;

            _logger.Log($"Process Upload Records");

            var actionCodes = _neutronVariables.ActionCodes.Split(',').Select(Parse).ToList();
            try
            {
                using (var db = new NeutronDb())
                {
                    var recs = db.History.Where(h => !h.TransmitDateTime.HasValue && actionCodes.Contains(h.ActionCode))
                        .ToList();
                    _logger.Log($"History Record Count: {recs.Count}");
                    if (recs.Count <= 0) return;
                    var hostFile = new HostFile(_neutronLicense, _neutronVariables, _rackStation);
                    hostFile.CreateMetHostFile(recs);

                    foreach (var rec in recs)
                    {
                        rec.TransmitDateTime = DateTime.Now;
                        _logger.Log($"Update the Transmit DateTime of History Record: {rec.TransmitDateTime}");
                    }

                    db.SaveChanges();

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"Upload Process Failed, see Log file in HostFile.");
                _logger.Log($"Create Host File Failed: {ex.Message} {Environment.NewLine} {ex.InnerException}");
            }
            finally
            {
                UploadBusy = false;
            }
            _logger.Log($"Set Upload Busy False");
            UploadBusy = false;
        }


        public void CreateReplenFile()
        {

            var counter = 0;
            while (ReplenBusy)
            {
                Thread.Sleep(1000);
                counter += 1;
                if (counter > 60)
                {
                    ReplenBusy = false;
                    break;
                }
            }

            ReplenBusy = true;

            _logger.Log($"Processing Highlight/Skip Records");
            try
            {
                using (var db = new NeutronDb())
                {
                    var recs = db.Database.SqlQuery<ReplenMilwaukeeTool>("usp_GetReplenishments").ToList();

                   // var recs = db.History.Where(h => !h.TransmitDateTime.HasValue && h.ActionCode == 45).ToList();
                    _logger.Log($"Replen Highlight Record Count: {recs.Count}");
                    if (recs.Count <= 0) return;
                    var directoryInfo = new DirectoryInfo(@"N:\REPLEN");
                    if (!Directory.Exists(directoryInfo.FullName))
                    {
                        Directory.CreateDirectory(directoryInfo.FullName);
                    }

                    const string fileName = @"Replen.csv";
                    var fullPath = Path.Combine(directoryInfo.FullName, fileName);

                    using (var sw = new StreamWriter(fullPath, true))
                    {
                        foreach (var rec in recs)
                        {
                            var sb = new StringBuilder();
                            sb.Append($"{rec.StationId}|");
                            sb.Append($"{rec.Item}|");
                            sb.Append($"{Convert.ToInt32(rec.RequestedQuantity)}|");
                            sb.Append($"{rec.Size}");
                            sw.WriteLine(sb.ToString());
                        }
                    }

                    foreach (var rec in recs)
                    {
                        var history = db.History.Find(rec.Id);
                        if(history!= null) history.TransmitDateTime = DateTime.Now;
                        _logger.Log($"Update the Transmit DateTime of History Record: {history.TransmitDateTime}");
                    }

                    db.SaveChanges();

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"Replen Process Failed, see Log file in HostFile.");
                _logger.Log($"Create Replen File Failed: {ex.Message} {Environment.NewLine} {ex.InnerException}");
            }
            finally
            {
                ReplenBusy = false;
            }
            _logger.Log($"Set Replen Busy False");
            ReplenBusy = false;
        }

    }
}
