using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NeutronLoader
{
    public static class LoaderSettings
    {

        public static string HostOrderDirectory { get; set; }
        public static string HostOrderFile { get; set; }
        public static string HostUploadDirectory { get; set; }
        public static string HostUploadFile { get; set; }
        public static string LogFileDirectory { get; set; }
        public static bool Initialized { get; set; }
        public static string EnableLogging { get; set; }

        public static void Save(string configFilePath)
        {
            using (StreamWriter sw = new StreamWriter(configFilePath))
            {
                sw.Write(string.Format("{0}{1}", HostOrderDirectory, '|'));
                sw.Write(string.Format("{0}{1}", HostOrderFile, '|'));
                sw.Write(string.Format("{0}{1}", HostUploadDirectory, '|'));
                sw.Write(string.Format("{0}{1}", HostUploadFile, '|'));
                sw.Write(string.Format("{0}{1}", EnableLogging, '|'));
                sw.Write(string.Format("{0}{1}", LogFileDirectory, '|'));
                sw.WriteLine();
            }
        }

        public static void Init(string configFilePath)
        {


            if (!File.Exists(configFilePath)) {
                HostOrderDirectory = String.Empty;
                HostOrderFile = String.Empty;
                HostUploadDirectory = String.Empty;
                HostUploadFile = String.Empty;
                LogFileDirectory = String.Empty;
                EnableLogging = "false";
                Save(configFilePath);
            }

            string line;
            using (StreamReader sr = new StreamReader(configFilePath))
            {
                line = sr.ReadLine();
                string[] tokens = line.Split('|');
                HostOrderDirectory = tokens[0];
                HostOrderFile = tokens[1];
                HostUploadDirectory = tokens[2];
                HostUploadFile = tokens[3];
                EnableLogging = tokens[4];
                LogFileDirectory = tokens[5];
            }
        }

        public static bool PathExists(string path)
        {
            bool result = false;
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    result = true;
                }
            }
            catch (Exception)
            {

            }
            return result;
        }
    }
}
