using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace AlliedLogger
{
    public class DynamicLogger
    {
        private string _filePath;
        private readonly object _myLock = new object();
        private static bool _init;

        public bool LogActivity { get; set; }
        public string FileName { get; set; }

        public DynamicLogger(string logFileDir, string folderName, string logActivity)
        {
            string baseFolder = string.IsNullOrEmpty(logFileDir) ? Environment.ExpandEnvironmentVariables(name: @"%SystemDrive%\NEUTRON\LOGS\") : logFileDir;
            baseFolder = baseFolder.EndsWith(@"\") ? baseFolder : baseFolder + @"\";
            folderName = folderName.EndsWith(@"\") ? folderName : folderName + @"\";
            LogActivity = logActivity == "true" ? true : false;
            FilePath = baseFolder + folderName + GetFileName();
        }

        public string FilePath
        {
            get
            {
                return _filePath;
            }
            set
            {
                if (value.Length > 0)
                {
                    _filePath = value;
                    var path = new FileInfo(_filePath);
                    try
                    {
                        // ... If the directory doesn't exist, create it.
                        if (!Directory.Exists(path.DirectoryName))
                        {
                            Directory.CreateDirectory(path.DirectoryName);
                        }
                    }
                    catch (Exception)
                    {
                        _init = false;
                    }
                }
                _init = true;
            }
        }

        public void Flush()
        {
            File.WriteAllText(_filePath, string.Empty);
        }

        public void Log(string msg)
        {
            string time = DateTime.Now.ToString("HH:mm:ss.fff");
            CultureInfo ci = CultureInfo.InvariantCulture;

            lock (_myLock)
            {
                if (msg.Length > 0)
                {
                    if (_init)
                    {
                        try
                        {
                            using (StreamWriter sw = File.AppendText(_filePath))
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
            DateTime now = DateTime.Now;
            string date = now.ToString("yyyyMMdd");

            return string.Concat(new string[] { date, ".Log" });
        }
    }
}

