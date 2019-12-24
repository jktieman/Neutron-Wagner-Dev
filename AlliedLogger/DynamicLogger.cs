using System;
using System.Globalization;
using System.IO;

namespace AlliedLogger
{
    public class DynamicLogger
    {
        private readonly object _myLock = new object();
        private static bool _validLocation;

        private readonly string _baseFolder;
        private readonly string _folderName;

        public bool LogActivity { get; set; }
        public string FileName { get; set; }

        public DynamicLogger(string logFileDir, string folderName = @"General\\", string logActivity = "true")
        {
            _baseFolder = string.IsNullOrEmpty(logFileDir) ? Environment.ExpandEnvironmentVariables(name: @"%SystemDrive%\NEUTRON\LOGS\") : logFileDir;
            _baseFolder = _baseFolder.EndsWith(@"\") ? _baseFolder : _baseFolder + @"\";
            _folderName = folderName.EndsWith(@"\") ? folderName : folderName + @"\";
            LogActivity = logActivity == "true";
            IsValidLocation();
        }

        public string FilePath => _baseFolder + _folderName + GetFileName();

        private void IsValidLocation()
        {
            _validLocation = false;
            var path = new FileInfo(FilePath);
            try
            {
                // ... If the directory doesn't exist, create it.
                if (!Directory.Exists(path.DirectoryName))
                {
                    if (path.DirectoryName != null) Directory.CreateDirectory(path.DirectoryName);
                }
                _validLocation = true;
            }
            catch (Exception)
            {
                _validLocation = false;
            }
        }

        public void Flush()
        {
            File.WriteAllText(FilePath, string.Empty);
        }

        public void Log(string msg)
        {
            var time = DateTime.Now.ToString("HH:mm:ss.fff");
            var ci = CultureInfo.InvariantCulture;

            lock (_myLock)
            {
                if (msg.Length > 0)
                {
                    if (_validLocation)
                    {
                        try
                        {
                            using (var sw = File.AppendText(FilePath))
                            {
                                sw.WriteLine("{0} {1}: {2}", DateTime.Now.ToShortDateString(), DateTime.Now.ToString("hh:mm:ss.FFF", ci), msg);
                                sw.Flush();
                            }
                        }
                        catch (Exception)
                        {
                            //silent fail
                        }
                    }
                }
            }
        }

        private string GetFileName()
        {
            var now = DateTime.Now;
            var date = now.ToString("yyyyMMdd");

            return string.Concat(new[] { date, ".Log" });
        }
    }
}

