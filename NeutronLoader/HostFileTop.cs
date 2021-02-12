//using Neutron.Models;
using NeutronCore;
using NeutronData.Models;
using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
//using Neutron.Global;
//using Neutron.Forms;

namespace NeutronLoader
{
    public class HostFileTop
    {
        private readonly DirectoryInfo _hostUploadDirectory;
        private readonly DirectoryInfo _logFileDirectory;
        private HostOrder _hostOrder;

        public HostFileTop(HostOrder order)
        {
            _hostOrder = order;
            //string configFilePath = String.Format("{0}", Properties.Settings.Default.ConfigFilePath);
            LoaderSettings.Init();

            _hostUploadDirectory = GetDirectory(LoaderSettings.GetHostUploadDirectory());
            if (_hostUploadDirectory != null)
            {
                SaveFile();
            }

            _logFileDirectory = GetDirectory(LoaderSettings.GetLogFileDirectory());
            if (_logFileDirectory != null && LoaderSettings.EnableLogging == "true")
            {
                SaveUploadLog();
            }
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

        private void SaveUploadLog()
        {
            if (!Directory.Exists(_logFileDirectory.FullName))
            {
                Directory.CreateDirectory(_logFileDirectory.FullName);
            }

            string fullName = $@"{_logFileDirectory.FullName}{GetLogFileName()}";
            try
            {
                var file = new FileInfo(fullName);

                if (!File.Exists(file.FullName))
                {
                    PrintHeading(ref file);
                }
                using (var tw = new StreamWriter(file.FullName, true))
                {
                    tw.WriteLine(GetCsvString());
                    tw.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($@"Host Upload Save File Error.  {ex.Message}");
            }
        }

        private void PrintHeading(ref FileInfo file)
        {

            try
            {

                using (var tw = new StreamWriter(file.FullName, true))
                {
                    tw.WriteLine(GetHeading());
                    tw.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($@"Host Upload Save File Error.  {ex.Message}");
            }
        }

        private string GetLogFileName()
        {
            DateTime now = DateTime.Now;
            string str = now.ToString("yyyyMMdd");
            return string.Format(format: "{0}.LOG", arg0: str);
        }

        public HostOrder HostOrder
        {
            get { return _hostOrder; }
            set { _hostOrder = value; }
        }

        private string GetFileName()
        {
            var d = DateTime.Now.ToString("yyyyMMddHHmmssfff"); // case sensitive
            string fileName = $"{_hostOrder.TypeCode}{d}.CSV";
            return fileName;
        }

        public void SaveFile()
        {
            if (!Directory.Exists(_hostUploadDirectory.FullName))
            {
                Directory.CreateDirectory(_hostUploadDirectory.FullName);
            }

            string fullName = $@"{_hostUploadDirectory.FullName}{GetFileName()}";

            try
            {
                using (var tw = new StreamWriter(fullName, true))
                {
                    tw.WriteLine(GetCsvString());
                    tw.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Host Upload Save File Error. " + ex.Message);
            }

        }

        private string GetCsvString()
        {
            var sb = new StringBuilder();
            sb.Append(_hostOrder.TypeCode + "|");
            sb.Append(_hostOrder.PartNum + "|");
            sb.Append(_hostOrder.PartDesc + "|");
            sb.Append(_hostOrder.JobNum + "|");
            sb.Append(_hostOrder.PrimeBin + "|");
            sb.Append(_hostOrder.NewBin + "|");
            sb.Append(_hostOrder.Qty + "|");
            sb.Append(_hostOrder.TroubleBit + "|");
            sb.Append(_hostOrder.DateTime + "|");
            sb.Append(_hostOrder.EmpId);
            sb.AppendLine();
            return sb.ToString();
        }


        private string GetHeading()
        {
            var sb = new StringBuilder();
            sb.Append("Type Code|");
            sb.Append("Part Num|");
            sb.Append("Part Desc|");
            sb.Append("Job Num|");
            sb.Append("Prime Bin|");
            sb.Append("NewBin|");
            sb.Append("Qty|");
            sb.Append("Trouble Bit|");
            sb.Append("Date Time|");
            sb.Append("Emp Id");
            sb.AppendLine();
            return sb.ToString();
        }
    }
}
