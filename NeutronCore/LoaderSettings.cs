using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using NeutronCore.Extensions;

namespace NeutronCore
{
    public static class LoaderSettings
    {
        private static string _rootDirectory = Environment.ExpandEnvironmentVariables(@"%SystemDrive%\Neutron\");
        private static readonly string backSlash = @"\";
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

        public static string GetImagesDirectory()
        {
            return PathExists(_imagesDirectory) ? _imagesDirectory : _rootDirectory;
        }

        public static void SetImagesDirectory(string value)
        {
            _imagesDirectory = value;
            if (value.Length > 0 && !value.EndsWith(backSlash))
            {
                _imagesDirectory = string.Concat(_imagesDirectory, backSlash);
            }
        }

        public static string GetHostOrderDirectory()
        {
            return PathExists(_hostOrderDirectory) ? _hostOrderDirectory : _rootDirectory;
        }

        public static void SetHostOrderDirectory(string value)
        {
            _hostOrderDirectory = value;
            if (value.Length > 0 && !value.EndsWith(backSlash))
            {
                _hostOrderDirectory = string.Concat(_hostOrderDirectory, backSlash);
            }
        }

        public static string GetRootDirectory()
        {
            return _rootDirectory;
        }

        public static void SetRootDirectory(string value)
        {
            _rootDirectory = value;
            if (value.Length > 0 && !value.EndsWith(backSlash))
            {
                _rootDirectory = string.Concat(_rootDirectory, backSlash);
            }
        }

        public static string GetHostOrderFile()
        {
            return _hostOrderFile;
        }

        public static void SetHostOrderFile(string value)
        {
            _hostOrderFile = value;
        }

        public static string GetHostOrderFileFilter()
        {
            return _hostOrderFileFilter;
        }

        public static void SetHostOrderFileFilter(string value)
        {
            _hostOrderFileFilter = value;
        }

        public static string GetHostUploadDirectory()
        {
            return PathExists(_hostUploadDirectory) ? _hostUploadDirectory : _rootDirectory;
        }

        public static void SetHostUploadDirectory(string value)
        {
            _hostUploadDirectory = value;
            if (value.Length > 0 && !value.EndsWith(backSlash))
            {
                _hostUploadDirectory = string.Concat(_hostUploadDirectory, backSlash);
            }
        }

        public static void SetCostCenterDirectory(string value)
        {
            _costCenterDirectory = value;
            if (value.Length > 0 && !value.EndsWith(backSlash))
            {
                _costCenterDirectory = string.Concat(_costCenterDirectory, backSlash);
            }
        }

        public static string GetCostCenterPath()
        {
            return GetCostCenterDirectory() + GetCostCenterFile();
        }

        public static string GetHostUploadFile()
        {
            return _hostUploadFile;
        }

        public static void SetHostUploadFile(string value)
        {
            _hostUploadFile = value;
        }

        public static void SetCostCenterFile(string value)
        {
            _costCenterFile = value;
        }

        public static string GetCostCenterFile()
        {
            return _costCenterFile;
        }

        public static string GetMaintenanceFileFilter()
        {
            return _maintenanceFileFilter;
        }

        public static void SetMaintenanceFileFilter(string value)
        {
            _maintenanceFileFilter = value;
        }

        public static string GetLogFileDirectory()
        {
            return PathExists(_logFileDirectory) ? _logFileDirectory : _rootDirectory;
        }

        public static void SetLogFileDirectory(string value)
        {
            _logFileDirectory = value;
            if (value.Length > 0 && !value.EndsWith(backSlash))
            {
                _logFileDirectory = string.Concat(_logFileDirectory, backSlash);
            }
        }

        public static string GetDocumentsDirectory()
        {
            return PathExists(_documentsDirectory) ? _documentsDirectory : _rootDirectory;
        }

        public static void SetDocumentsDirectory(string value)
        {
            _documentsDirectory = value;
            if (value.Length > 0 && !value.EndsWith(backSlash))
            {
                _documentsDirectory = string.Concat(_documentsDirectory, backSlash);
            }
        }

        public static string GetMaintenanceFileDirectory()
        {
            return PathExists(_maintenanceFileDirectory) ? _maintenanceFileDirectory : _rootDirectory;
        }

        public static string GetCostCenterDirectory()
        {
            return PathExists(_costCenterDirectory) ? _costCenterDirectory : _rootDirectory;
        }

        public static void SetMaintenanceFileDirectory(string value)
        {
            _maintenanceFileDirectory = value;
            if (value.Length > 0 && !value.EndsWith(backSlash))
            {
                _maintenanceFileDirectory = string.Concat(_maintenanceFileDirectory, backSlash);
            }
        }

        public static string GetLanguageDirectory()
        {
            if (string.IsNullOrEmpty(_languageDirectory))
            {
                _languageDirectory = _rootDirectory + @"Language\";
            }
            return _languageDirectory;
        }

        public static void SetLanguageDirectory(string value)
        {
            _languageDirectory = value;
            if (value.Length > 0 && !value.EndsWith(backSlash))
            {
                _languageDirectory = string.Concat(_languageDirectory, backSlash);
            }
        }

        public static void Save(string configFilePath)
        {
            using (StreamWriter sw = new StreamWriter(configFilePath))
            {
                sw.Write($"{_imagesDirectory}{'|'}");
                sw.Write($"{_hostOrderDirectory}{'|'}");
                sw.Write($"{_hostOrderFile}{'|'}");
                sw.Write($"{_hostUploadDirectory}{'|'}");
                sw.Write($"{_hostUploadFile}{'|'}");
                sw.Write($"{EnableLogging}{'|'}");
                sw.Write($"{_logFileDirectory}{'|'}");
                sw.Write($"{_hostOrderFileFilter}{'|'}");
                sw.Write($"{_maintenanceFileFilter}{'|'}");
                sw.Write($"{_documentsDirectory}{'|'}");
                sw.Write($"{_maintenanceFileDirectory}{'|'}");
                sw.Write($"{_costCenterDirectory}{'|'}");
                sw.Write($"{_costCenterFile}{'|'}");
                sw.Write($"{_languageDirectory}");
                sw.WriteLine();
            }
        }

        public static void Init(string configFilePath)
        {
           // var result = false;
            try
            {
                if (PathExists(_rootDirectory))
                {
                    if (!File.Exists(configFilePath))
                    {
                        SetImagesDirectory(string.Empty);
                        SetHostOrderDirectory(string.Empty);
                        SetHostOrderFile(string.Empty);
                        SetHostUploadDirectory(string.Empty);
                        SetHostUploadFile(string.Empty);
                        EnableLogging = "false";
                        SetLogFileDirectory(string.Empty);
                        SetHostOrderFileFilter(string.Empty);
                        SetMaintenanceFileFilter(string.Empty);
                        SetDocumentsDirectory(string.Empty);
                        SetMaintenanceFileDirectory(string.Empty);
                        SetCostCenterDirectory(string.Empty);
                        SetCostCenterFile(string.Empty);
                        SetLanguageDirectory(string.Empty);
                        Save(configFilePath);
                    }

                    using (var sr = new StreamReader(configFilePath))
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
                            EnableLogging = tokens[5];
                            SetLogFileDirectory(tokens[6]);
                            SetHostOrderFileFilter(tokens[7]);
                            SetMaintenanceFileFilter(tokens[8]);
                            SetDocumentsDirectory(tokens[9]);
                            SetMaintenanceFileDirectory(tokens[10]);
                            SetCostCenterDirectory(tokens[11]);
                            SetCostCenterFile(tokens[12]);
                            SetLanguageDirectory(tokens[13]);
                        }
                    }
                    //result = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Check the Loader Settings file for missing fields.  {ex.Message}  {ex.InnerException}");
               // result = false;
            }
            //return result;
        }

        public static bool PathExists(string path)
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
