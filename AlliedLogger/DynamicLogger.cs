using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AlliedLogger
{
    public class DynamicLogger : IDynamicLogger
    {
        private readonly object _myLock = new object();
        private static bool _validLocation;
        private string _baseFolder;
        private string _folderName;
        private static readonly object MyLock = new object();
        private ConcurrentQueue<string> _messages = new ConcurrentQueue<string>();

        //public string FolderName { get; set; }
        //public bool LogActivity { get; set; }
        public string LogActivity { get; set; }

        public string FileName { get; set; }

        public DynamicLogger(string logFileDir = "", string folderName = @"General", string logActivity = "true")
        {
            LogFileDir = logFileDir;
            FolderName = folderName;
            //_baseFolder = string.IsNullOrEmpty(logFileDir) ? Environment.ExpandEnvironmentVariables(name: @"%SystemDrive%\NEUTRON\LOGS\") : logFileDir;
            //_baseFolder = _baseFolder.EndsWith(@"\") ? _baseFolder : _baseFolder + @"\";
            // _folderName = folderName.EndsWith(@"\") ? folderName : folderName + @"\";
            LogActivity = logActivity;
            IsValidLocation(FilePath);
            IsValidLocation(TempFilePath);
        }

        public string LogFileDir
        {
            get
            {
                return _baseFolder;
            }
            set
            {
                _baseFolder = string.IsNullOrEmpty(value) ? Environment.ExpandEnvironmentVariables(name: @"%SystemDrive%\NEUTRON\LOGS\") : value;
                _baseFolder = _baseFolder.EndsWith(@"\") ? _baseFolder : _baseFolder + @"\";
            }
        }

        public string FolderName
        {
            get
            {
                return _folderName;
            }
            set
            {
                _folderName = value.EndsWith(@"\") ? value : value + @"\";
            }
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
            IsValidLocation(FilePath);
            IsValidLocation(TempFilePath);
            if (!_validLocation) return;
            lock (_myLock)
            {
                if (msg.Length > 0)
                {

                    try
                    {
                        using (var sw = File.AppendText(FilePath))
                        {
                            sw.WriteLine("{0} {1}: {2}", DateTime.Now.ToShortDateString(),
                                DateTime.Now.ToString("hh:mm:ss.FFF", ci), msg);
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

        public async Task LogAsync(string msg)
        {
            var time = DateTime.Now.ToString("HH:mm:ss.fff");
            var ci = CultureInfo.InvariantCulture;
            IsValidLocation(FilePath);
            IsValidLocation(TempFilePath);
            if (!_validLocation) return;
            if (msg.Length > 0)
            {
                try
                {
                    using (var sw = File.AppendText(FilePath))
                    {
                        await sw.WriteLineAsync($"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToString("hh:mm:ss.FFF", ci)}: {msg}");
                        await sw.FlushAsync();
                    }
                }
                catch (Exception)
                {
                    //silent fail
                }
            }
        }

        public void LogDetail(string msg = ""
            , [CallerMemberName] string origin = ""
            , [CallerFilePath] string filePath = ""
            , [CallerLineNumber] int lineNumber = 0)
        {
            var ci = CultureInfo.InvariantCulture;
            IsValidLocation(FilePath);
            IsValidLocation(TempFilePath);
            if (!_validLocation) return;

            if (msg.Length <= 0) return;
            try
            {
                using (var sw = File.AppendText(FilePath))
                {
                    var message =
                        $"[{Path.GetFileName(filePath)} > {origin}() > Line: {lineNumber}] {Environment.NewLine}{msg}";

                    sw.WriteLineAsync($"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToString("hh:mm:ss.FFF", ci)}: {message} {Environment.NewLine}");
                    sw.FlushAsync();
                }
            }
            catch (Exception ex)
            {
                //silent fail
                Log($"Detail Error: {ex.Message}");
            }
        }
            private bool _inProcess = false;
        
            public async void LogDetailAsync(string msg = ""
            , [CallerMemberName] string origin = ""
            , [CallerFilePath] string filePath = ""
            , [CallerLineNumber] int lineNumber = 0)
        {

            var ci = CultureInfo.InvariantCulture;
            IsValidLocation(FilePath);
            IsValidLocation(TempFilePath);
            if (!_validLocation) return;

            if (msg.Length <= 0) return;

            var message =
                $"[{Path.GetFileName(filePath)} > {origin}() > Line: {lineNumber}] {Environment.NewLine}{msg}";
            
            _messages.Enqueue(message);

            try
            {
                if (!_inProcess)
                {
                    _inProcess = true;

                    using (var sw = File.AppendText(FilePath))
                    {
                        while (!_messages.IsEmpty)
                        {
                            _messages.TryDequeue(out msg);

                        await sw.WriteLineAsync($"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToString("hh:mm:ss.FFF", ci)}: {msg} {Environment.NewLine}");
                        await sw.FlushAsync();
                        }
                    }
                    _inProcess = false;
                }
            }
            catch (Exception ex)
            {
                Log($"Detail Async Error: {ex.Message}");
                //silent fail
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

