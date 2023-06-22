using AlliedLogger;
using NeutronCore.Global;
using NeutronCore.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NeutronCore;
using NeutronData.DataContexts;
using NeutronData.Interfaces;
using NeutronData.Models;
using NeutronData.ModelViews;
using static System.Int32;
using Timer = System.Threading.Timer;

namespace NeutronLoader
{
    public class UploadProcessorPr1 : IUploadProcessor
    {
        private readonly NeutronLicense _neutronLicense;
        private readonly NeutronVariables _neutronVariables;
        private readonly DynamicLogger _logger;
        private readonly WorkstationView _workstationView;
        private readonly IWorkstationRepository _workstationRepository;
        private Timer _timer;
        private bool _uploadBusy;
        private DirectoryInfo _hostUploadDirectory;

        public UploadProcessorPr1(NeutronVariables neutronVariables, NeutronLicense neutronLicense,
            DynamicLogger logger, WorkstationView workstationView, IWorkstationRepository workstationRepository)
        {
            _neutronLicense = neutronLicense;
            _neutronVariables = neutronVariables;
            _logger = logger;
            _workstationView = workstationView;
            _workstationRepository = workstationRepository;
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

        private DirectoryInfo GetDirectory(string dir)
        {
            DirectoryInfo result = null;

            if (!string.IsNullOrEmpty(dir))
            {
                result = new DirectoryInfo(dir);
            }
            return result;
        }

        public void CreateHostFile()
        {
            var appendFile = Convert.ToBoolean(LoaderSettings.AppendFile);
            _hostUploadDirectory = GetDirectory(LoaderSettings.GetHostUploadDirectory());
            if (!Directory.Exists(_hostUploadDirectory.FullName))
            {
                Directory.CreateDirectory(_hostUploadDirectory.FullName);
            }
            var fileName = LoaderSettings.GetHostUploadFile();
            if (!string.IsNullOrEmpty(fileName))
            {
                var fullName = Path.Combine(_hostUploadDirectory.FullName, fileName);
                if (File.Exists(fullName))
                {
                    if(!appendFile)
                    {
                        MessageBox.Show("Upload file exists.", "File Exists", MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        return;
                    }
                }
            }

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
                        var hostFile = new HostFilePr1(_neutronLicense, _neutronVariables, _workstationRepository);
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
