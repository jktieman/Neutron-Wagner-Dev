using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace AlliedLogger
{
    public static class Logger
    {
        private static string logFilePath;
        private static readonly Object mylock = new Object();
        private static bool init;
        private static bool logActivity;

        public static bool LogActivity
        {
            get { return logActivity; }
            set { logActivity = value; }
        }

        public static string FilePath
        {
            get
            {
                return Logger.logFilePath;
            }
            set
            {
                if (value.Length > 0)
                {
                    Logger.logFilePath = value;
                    var path = new FileInfo(Logger.logFilePath);
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
                        init = false;
                    }
                }
                init = true;
            }
        }

        public static void Flush()
        {
            File.WriteAllText(Logger.FilePath, string.Empty);
        }

        public static void Log(string msg)
        {
            if (logActivity)
            {

                CultureInfo ci = CultureInfo.InvariantCulture;

                lock (mylock)
                {
                    if (msg.Length > 0)
                    {
                        if (init)
                        {
                            try
                            {
                                using (StreamWriter sw = File.AppendText(Logger.FilePath))
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
