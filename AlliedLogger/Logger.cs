using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace AlliedLogger
{
    public static class Logger
    {
        private static string _logFilePath;
        private static readonly object Mylock = new object();
        private static bool _validPath;

        public static bool LogActivity { get; set; }

        public static string FilePath
        {
            get
            {
                return _logFilePath;
            }
            set
            {
                lock (Mylock)
                {
                    _validPath = false;

                    if (!string.IsNullOrEmpty(value))
                    {
                        _logFilePath = value;
                        var path = new FileInfo(_logFilePath);
                        var directory = path.DirectoryName;
                        try
                        {
                            // ... If the directory doesn't exist, create it.
                            if (directory != null)
                            {
                                if (!Directory.Exists(directory))
                                {
                                    Directory.CreateDirectory(directory);
                                    _validPath = true;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            // Silent Fail
                        }
                    }
                }
            }
        }

        public static void Flush()
        {
            File.WriteAllText(_logFilePath, string.Empty);
        }

        public static void Log(string msg)
        {
            if (LogActivity)
            {
                CultureInfo ci = CultureInfo.InvariantCulture;
                lock (Mylock)
                {
                    if (msg.Length > 0)
                    {
                        if (_validPath)
                        {
                            try
                            {
                                using (var sw = File.AppendText(_logFilePath))
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
        }
    }
}
