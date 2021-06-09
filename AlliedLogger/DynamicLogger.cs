using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

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
            IsValidLocation(FilePath);
            IsValidLocation(TempFilePath);
        }

        public string FilePath => _baseFolder + _folderName + GetFileName();
        public string TempFilePath => $"{_baseFolder}{_folderName}Temp\\{GetFileName()}";

        private void IsValidLocation(string filePath)
        {
            _validLocation = false;
            var path = new FileInfo(filePath);
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

        public List<string> LastLogLines(int numLines = 10)
        {
            CreateTempLog();
            if (!File.Exists(TempFilePath)) return new List<string>();


            var allLines = File.ReadLines(TempFilePath).ToArray();
            var numLinesCount = allLines.Count();
            var lines = new List<string>();
            var linesToRead = numLinesCount > numLines ? numLines : numLinesCount;
            if (linesToRead == 0) return lines;

            for (var i = numLinesCount - linesToRead; i < numLinesCount; i++)
            {
                lines.Add($"{allLines[i]}{Environment.NewLine}");
            }

            return lines;
        }

        //public string TempFilePath
        //{
        //    get
        //    {
        //        return $"{_tempDirectoryInfo.FullName}{GetFileName()}";
        //    }
        //    set
        //    {
        //        if (value.Length > 0)
        //        {
        //            _fileInfo = new FileInfo(value);
        //            var dir = $"{_fileInfo.DirectoryName}\\LoaderLogs\\Temp\\";
        //            if (!Directory.Exists(dir))
        //            {
        //                _tempDirectoryInfo = Directory.CreateDirectory(dir);
        //            }
        //            else
        //            {
        //                _tempDirectoryInfo = new DirectoryInfo(dir);
        //            }
        //        }
        //    }
        //}

        private void CreateTempLog()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    File.Copy(FilePath, TempFilePath, true);
                }


            }
            catch (Exception ex)
            {

                Log($"{ex.Message}{ex.InnerException}");
            };
        }
    }
}

