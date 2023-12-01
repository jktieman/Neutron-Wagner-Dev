using NeutronEvents;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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
            // Send all logs to the same file
            //LogFileDir = null;
            //FolderName = @"SingleLogFile\";

            // Use these to create separate log files by file name
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
            catch (Exception ex)
            {
                _validLocation = false;
                ErrorAlert($"Error Testing for a Valid Directory: {filePath}: {ex.Message}");
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

        public async Task LogDetailAsync(string msg = ""
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
                    if (!IsFileLocked(FilePath, 5))
                    {
                        _inProcess = true;
                        using (var sw = new StreamWriter(FilePath, true))
                        {
                            try
                            {
                                while (!_messages.IsEmpty)
                                {
                                    _messages.TryDequeue(out msg);

                                    await sw.WriteLineAsync($"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToString("hh:mm:ss.FFF", ci)}: {msg} {Environment.NewLine}");
                                    await sw.FlushAsync();
                                }
                            }
                            catch (Exception ex)
                            {
                                throw new Exception($"Detail Async Error: {ex.Message}");
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("File is locked");
                    }
                    _inProcess = false;
                }
            }
            catch (Exception ex)
            {
                _messages.Enqueue($"Detail Async Error: {ex.Message}");
            }
        }

        public bool IsFileLocked(string filePath, int secondsToWait)
        {
            bool isLocked = true;
            int i = 0;

            while (isLocked && ((i < secondsToWait) || (secondsToWait == 0)))
            {
                try
                {
                    using (File.Open(filePath, FileMode.Open)) { }
                    return false;
                }
                catch (IOException e)
                {
                    var errorCode = Marshal.GetHRForException(e) & ((1 << 16) - 1);
                    isLocked = errorCode == 32 || errorCode == 33;
                    i++;

                    if (secondsToWait != 0)
                        new System.Threading.ManualResetEvent(false).WaitOne(1000);
                }
            }

            return isLocked;
        }

        private string GetFileName()
        {
            var now = DateTime.Now;
            var date = now.ToString("yyyyMMdd");

            return string.Concat(new[] { date, ".Log" });
        }

        //public List<string> LastLogLines(int numLines = 10)
        //{
        //    CreateTempLog();
        //    if (!File.Exists(TempFilePath)) return new List<string>();


        //    var allLines = File.ReadLines(TempFilePath).ToArray();
        //    var numLinesCount = allLines.Count();
        //    var lines = new List<string>();
        //    var linesToRead = numLinesCount > numLines ? numLines : numLinesCount;
        //    if (linesToRead == 0) return lines;

        //    for (var i = numLinesCount - linesToRead; i < numLinesCount; i++)
        //    {
        //        lines.Add($"{allLines[i]}{Environment.NewLine}");
        //    }

        //    return lines;
        //}

        public StringBuilder LastLogLines(int lines = 10)
        {
            var sb = new StringBuilder();
            var counter = 0;
            try
            {
                // check to see if file is open
                while (IsFileOpen(FilePath))
                {
                    Thread.Sleep(100);
                    counter += 1;
                }


                var allLines = File.ReadLines(FilePath).ToArray();
                var numLinesCount = allLines.Count();

                var linesToRead = numLinesCount > lines ? lines : numLinesCount;
                if (linesToRead == 0) return sb;

                for (var i = numLinesCount - linesToRead; i < numLinesCount; i++)
                {
                    sb.AppendLine(allLines[i]);
                }
            }
            catch (Exception ex)
            {
                ErrorAlert($"Last Log Lines Error: {ex.Message}");
                throw;
            }

            return sb;
        }
        private bool IsFileOpen(string filePath)
        {
            try
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite))
                {
                    // The file is not open
                    return false;
                }
            }
            catch (IOException)
            {
                // The file is open
                return true;
            }
        }
        
        private void CloseFile(string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite))
            {
                // Do something with the file
            }

            // The file is now closed
        }
        public void ErrorAlert(string err)
        {
            Mediator.GetInstance().OnLoaderError(this, err);
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

