using AlliedLogger;
using NeutronCore.Global;
using NeutronCore.Models;
using NeutronData.Models;
using NeutronData.ModelViews;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AsyncAwaitBestPractices;
using Timer = System.Threading.Timer;
using static System.Int32;
using NeutronData.DataContexts;

namespace NeutronLoader
{
    public class UploadProcessorMet : IUploadProcessor
    {
        private readonly NeutronLicense _neutronLicense;
        private readonly NeutronVariables _neutronVariables;
        private readonly IDynamicLogger _logger;
        private readonly WorkstationView _workstationView;
        private Timer _timer;
        private Timer _replenTimer;

        public bool UploadBusy;
        public bool ReplenBusy;

        public UploadProcessorMet(NeutronVariables neutronVariables, NeutronLicense neutronLicense,
            IDynamicLogger logger, WorkstationView workstationView)
        {
            _neutronLicense = neutronLicense;
            _neutronVariables = neutronVariables;
            _logger = logger;
            _workstationView = workstationView;

        }


        public async Task RunUploadOnce()
        {
           await CreateHostFile();
            CreateReplenFile();
        }

        public void StartProcessingUploadFiles()
        {
            _logger.LogDetailAsync($"Start Processing Upload Files").SafeFireAndForget();
            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromSeconds(_neutronVariables.UploadDelay);
            _timer = new Timer(t => { CreateHostFile().SafeFireAndForget(); }, null, startTimeSpan, periodTimeSpan);
            _logger.LogDetailAsync($"Start Processing Skip Replenishment").SafeFireAndForget();
            startTimeSpan = TimeSpan.Zero;
            periodTimeSpan = TimeSpan.FromSeconds(_neutronVariables.UploadDelay);
            _replenTimer = new Timer(t => { CreateReplenFile(); }, null, startTimeSpan, periodTimeSpan);

        }

        public void StopProcessingUploadFiles()
        {
            _timer?.Dispose();
            _replenTimer?.Dispose();
        }

        public async Task CreateHostFile()
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

            _logger.LogDetailAsync($"Process Upload Records").SafeFireAndForget();

            var actionCodes = _neutronVariables.ActionCodes.Split(',').Select(Parse).ToList();
            try
            {
                using (var db = new NeutronDb())
                {
                    var recs = db.History.Where(h => !h.TransmitDateTime.HasValue && actionCodes.Contains(h.ActionCode))
                        .ToList();
                    _logger.LogDetailAsync($"History Record Count: {recs.Count}").SafeFireAndForget();
                    if (recs.Count <= 0) return;
                    var hostFile = new HostFile(_neutronLicense, _neutronVariables, _workstationView);
                    hostFile.CreateMetHostFile(recs);

                    foreach (var rec in recs)
                    {
                        rec.TransmitDateTime = DateTime.Now;
                        _logger.LogDetailAsync($"Update the Transmit DateTime of History Record: {rec.TransmitDateTime}").SafeFireAndForget();
                    }

                    await db.SaveChangesAsync();

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"Upload Process Failed, see Log file in HostFile.");
                _logger.LogDetailAsync($"Create Host File Failed: {ex.Message} {Environment.NewLine} {ex.InnerException}").SafeFireAndForget();
            }
            finally
            {
                UploadBusy = false;
            }
            _logger.LogDetailAsync($"Set Upload Busy False").SafeFireAndForget();
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

            _logger.LogDetailAsync($"Processing Highlight/Skip Records").SafeFireAndForget();
            try
            {
                using (var db = new NeutronDb())
                {
                    var recs = db.Database.SqlQuery<ReplenMilwaukeeTool>("usp_GetReplenishments").ToList();

                    // var recs = db.History.Where(h => !h.TransmitDateTime.HasValue && h.ActionCode == 45).ToList();
                    _logger.LogDetailAsync($"Replen Highlight Record Count: {recs.Count}").SafeFireAndForget();
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
                            sb.Append($"{rec.AreaId}|");
                            sb.Append($"{rec.Item}|");
                            sb.Append($"{rec.RequestedQuantity}|");
                            sb.Append($"{rec.Size}");
                            sw.WriteLine(sb.ToString());
                        }
                    }

                    foreach (var rec in recs)
                    {
                        var history = db.History.Find(rec.Id);
                        if (history == null) continue;
                        history.TransmitDateTime = DateTime.Now;
                        _logger.LogDetailAsync($"Update the Transmit DateTime of History Record: {history.TransmitDateTime}").SafeFireAndForget();
                    }

                    db.SaveChanges();

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"Replen Process Failed, see Log file in HostFile.");
                _logger.LogDetailAsync($"Create Replen File Failed: {ex.Message} {Environment.NewLine} {ex.InnerException}").SafeFireAndForget();
            }
            finally
            {
                ReplenBusy = false;
            }
            _logger.LogDetailAsync($"Set Replen Busy False").SafeFireAndForget();
            ReplenBusy = false;
        }

    }
}
