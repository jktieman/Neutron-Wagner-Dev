using System;
using AlliedLogger;

namespace NeutronCore.Global
{
    public static class Logger
    {
        private static IDynamicLogger _logger = null;
        public static IDynamicLogger SetupLogger(string folderName)
        {
            if (_logger != null) return _logger;
            try

            {
                //logger = new DynamicLogger();
                var logFileDir = LoaderSettings.GetLogFileDirectory();
                var logActivity = LoaderSettings.EnableLogging;
                _logger = new DynamicLogger(logFileDir, folderName, logActivity);
                _logger.LogDetailAsync("Logger setup complete.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logger setup error: {ex.Message}");
            }

            return _logger;
        }
    }
}
