using System;
using System.IO;
using System.Windows.Forms;

namespace NeutronCore
{
    public static class LoaderSettings
    {
        private static string _commonDirectory;
        private static string _rootDirectory;
        private const string SubDirectory = @"Configuration\";
        private const string FileName = @"ConfigFile.Csv";
        private const string BackSlash = @"\";
        private static string _imagesDirectory;
        private static string _hostOrderDirectory;
        private static string _hostOrderFile;
        private static string _hostOrderFileFilter;
        private static string _hostUploadDirectory;
        private static string _hostUploadFile;
        private static string _maintenanceFileFilter;
        private static string _logFileDirectory;
        private static string _documentsDirectory;
        private static string _maintenanceFileDirectory;
        private static string _costCenterDirectory;
        private static string _costCenterFile;
        private static string _languageDirectory;

        public static bool Initialized { get; set; }
        public static string EnableLogging { get; set; }
        public static string AppendFile { get; set; }

        static LoaderSettings()
        {
            _rootDirectory = Environment.ExpandEnvironmentVariables(@"%SystemDrive%\Neutron\");

            if (!Directory.Exists(_rootDirectory))
            {
                _rootDirectory = string.Empty;
            }

            Init();
        }
        public static string ConfigFilePath => $"{GetRootDirectory()}{SubDirectory}{FileName}";
        public static string GetImagesDirectory() => PathExists(_imagesDirectory) ? _imagesDirectory : _rootDirectory;
        public static void SetImagesDirectory(string value)
        {
            _imagesDirectory = value;
            if (value.Length > 0 && !value.EndsWith(BackSlash))
            {
                _imagesDirectory = string.Concat(_imagesDirectory, BackSlash);
            }
        }
        public static string GetHostOrderDirectory() => PathExists(_hostOrderDirectory) ? _hostOrderDirectory : _rootDirectory;
        public static void SetHostOrderDirectory(string value)
        {
            _hostOrderDirectory = value;
            if (value.Length > 0 && !value.EndsWith(BackSlash))
            {
                _hostOrderDirectory = string.Concat(_hostOrderDirectory, BackSlash);
            }
        }
        public static string GetCommonDirectory() => PathExists(_commonDirectory) ? _commonDirectory : _rootDirectory;
        public static void SetCommonDirectory(string value)
        {
            _commonDirectory = value;
            if (value.Length > 0 && !value.EndsWith(BackSlash))
            {
                _commonDirectory = string.Concat(_commonDirectory, BackSlash);
            }
        }
        public static string GetRootDirectory() => _rootDirectory;
        public static void SetRootDirectory(string value)
        {
            _rootDirectory = value;
            if (value.Length > 0 && !value.EndsWith(BackSlash))
            {
                _rootDirectory = string.Concat(_rootDirectory, BackSlash);
            }
        }
        public static string GetHostOrderFile() => _hostOrderFile;
        public static void SetHostOrderFile(string value) => _hostOrderFile = value;
        public static string GetHostOrderFileFilter() => _hostOrderFileFilter;
        public static void SetHostOrderFileFilter(string value) => _hostOrderFileFilter = value;
        public static string GetHostUploadDirectory() => PathExists(_hostUploadDirectory) ? _hostUploadDirectory : _rootDirectory;
        public static void SetHostUploadDirectory(string value)
        {
            _hostUploadDirectory = value;
            if (value.Length > 0 && !value.EndsWith(BackSlash))
            {
                _hostUploadDirectory = string.Concat(_hostUploadDirectory, BackSlash);
            }
        }
        public static void SetCostCenterDirectory(string value)
        {
            _costCenterDirectory = value;
            if (value.Length > 0 && !value.EndsWith(BackSlash))
            {
                _costCenterDirectory = string.Concat(_costCenterDirectory, BackSlash);
            }
        }
        public static string GetCostCenterPath() => GetCostCenterDirectory() + GetCostCenterFile();
        public static string GetHostUploadFile() => _hostUploadFile;
        public static void SetHostUploadFile(string value) => _hostUploadFile = value;
        public static void SetCostCenterFile(string value) => _costCenterFile = value;
        public static string GetCostCenterFile() => _costCenterFile;
        public static string GetMaintenanceFileFilter() => _maintenanceFileFilter;
        public static void SetMaintenanceFileFilter(string value) => _maintenanceFileFilter = value;
        public static string GetLogFileDirectory() => PathExists(_logFileDirectory) ? _logFileDirectory : _rootDirectory + @"Logs\";
        public static void SetLogFileDirectory(string value)
        {
            if (PathExistsCreate(value))
            {
                _logFileDirectory = value;
                if (value.Length > 0 && !value.EndsWith(BackSlash))
                {
                    _logFileDirectory = string.Concat(_logFileDirectory, BackSlash);
                }
            }
            else
            {
                MessageBox.Show("Directory does not exists. Cannot create Log directory.");
            }

        }
        public static string GetDocumentsDirectory() => PathExists(_documentsDirectory) ? _documentsDirectory : _rootDirectory;
        public static void SetDocumentsDirectory(string value)
        {
            _documentsDirectory = value;
            if (value.Length > 0 && !value.EndsWith(BackSlash))
            {
                _documentsDirectory = string.Concat(_documentsDirectory, BackSlash);
            }
        }
        public static string GetMaintenanceFileDirectory() => PathExists(_maintenanceFileDirectory) ? _maintenanceFileDirectory : _rootDirectory;
        public static string GetCostCenterDirectory() => PathExists(_costCenterDirectory) ? _costCenterDirectory : _rootDirectory;
        public static void SetMaintenanceFileDirectory(string value)
        {
            _maintenanceFileDirectory = value;
            if (value.Length > 0 && !value.EndsWith(BackSlash))
            {
                _maintenanceFileDirectory = string.Concat(_maintenanceFileDirectory, BackSlash);
            }
        }
        public static string GetLanguageDirectory()
        {
            if (string.IsNullOrEmpty(_languageDirectory))
            {
                _languageDirectory = @"Language\";
                // _languageDirectory = _rootDirectory + @"Language\";
            }
            return _languageDirectory;
        }

        public static void SetLanguageDirectory(string value)
        {
            _languageDirectory = value;
            if (value.Length > 0 && !value.EndsWith(BackSlash))
            {
                _languageDirectory = string.Concat(_languageDirectory, BackSlash);
            }
        }

        public static void Save()
        {
            var path = $"{GetRootDirectory()}{SubDirectory}";
            if (PathExists(path))
            {
                using (StreamWriter sw = new StreamWriter(ConfigFilePath))
                {
                    sw.Write($"{_imagesDirectory}{'|'}");
                    sw.Write($"{_hostOrderDirectory}{'|'}");
                    sw.Write($"{_hostOrderFile}{'|'}");
                    sw.Write($"{_hostUploadDirectory}{'|'}");
                    sw.Write($"{_hostUploadFile}{'|'}");
                    sw.Write($"{AppendFile}{'|'}");
                    sw.Write($"{EnableLogging}{'|'}");
                    sw.Write($"{_logFileDirectory}{'|'}");
                    sw.Write($"{_hostOrderFileFilter}{'|'}");
                    sw.Write($"{_maintenanceFileFilter}{'|'}");
                    sw.Write($"{_documentsDirectory}{'|'}");
                    sw.Write($"{_maintenanceFileDirectory}{'|'}");
                    sw.Write($"{_costCenterDirectory}{'|'}");
                    sw.Write($"{_costCenterFile}{'|'}");
                    sw.Write($"{_languageDirectory}{'|'}");
                    sw.Write($"{_commonDirectory}{'|'}");
                    sw.Write($"{_rootDirectory}");
                    sw.WriteLine();
                }
            }
        }

        public static bool Init()
        {
            var result = false;

            var path = $"{GetRootDirectory()}{SubDirectory}";
            if (PathExists(path))
            {
                try
                {
                    if (!File.Exists(ConfigFilePath))
                    {
                        SetImagesDirectory(string.Empty);
                        SetHostOrderDirectory(string.Empty);
                        SetHostOrderFile(string.Empty);
                        SetHostUploadDirectory(string.Empty);
                        SetHostUploadFile(string.Empty);
                        AppendFile = "false";
                        EnableLogging = "false";
                        SetLogFileDirectory(string.Empty);
                        SetHostOrderFileFilter(string.Empty);
                        SetMaintenanceFileFilter(string.Empty);
                        SetDocumentsDirectory(string.Empty);
                        SetMaintenanceFileDirectory(string.Empty);
                        SetCostCenterDirectory(string.Empty);
                        SetCostCenterFile(string.Empty);
                        SetLanguageDirectory(string.Empty);
                        SetCommonDirectory(string.Empty);
                        SetRootDirectory(string.Empty);
                        Save();
                    }

                    using (var sr = new StreamReader(ConfigFilePath))
                    {
                        var line = sr.ReadLine();
                        if (line != null)
                        {
                            var tokens = line.Split('|');
                            SetImagesDirectory(tokens[0]);
                            SetHostOrderDirectory(tokens[1]);
                            SetHostOrderFile(tokens[2]);
                            SetHostUploadDirectory(tokens[3]);
                            SetHostUploadFile(tokens[4]);
                            AppendFile = tokens[5];
                            EnableLogging = tokens[6];
                            SetLogFileDirectory(tokens[7]);
                            SetHostOrderFileFilter(tokens[8]);
                            SetMaintenanceFileFilter(tokens[9]);
                            SetDocumentsDirectory(tokens[10]);
                            SetMaintenanceFileDirectory(tokens[11]);
                            SetCostCenterDirectory(tokens[12]);
                            SetCostCenterFile(tokens[13]);
                            SetLanguageDirectory(tokens[14]);
                            SetCommonDirectory(tokens[15]);
                            SetRootDirectory(tokens[16]);
                        }
                    }

                    result = true;
                }

                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Check the Loader Settings file for missing fields.  {ex.Message}  {ex.InnerException}");
                    result = false;
                }
            }

            return result;
        }

        public static bool PathExists(string path)
        {
            var result = false;
            try
            {
                if (path.Length > 0)
                {
                    if (Directory.Exists(path))
                    {
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Directory does not exists. Add in System Interface. " + ex.Message + " " + ex.InnerException);
                result = false;
            }
            return result;
        }

        public static bool PathExistsCreate(string path)
        {
            var result = false;
            try
            {
                if (path.Length > 0)
                {
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    result = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Directory does not exists. Add in System Interface. " + ex.Message + " " + ex.InnerException);
                result = false;
            }
            return result;
        }

    }
}
