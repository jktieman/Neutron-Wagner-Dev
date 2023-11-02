using System;
using AlliedLogger;

namespace NeutronCore.Global
{
    public static class Logger
    {
        private static IDynamicLogger _logger = null;
        public static IDynamicLogger SetupLogger(string folderName)
        {
            try
            {
                var logFileDir = LoaderSettings.GetLogFileDirectory();
                var logActivity = LoaderSettings.EnableLogging;
                _logger = new DynamicLogger(logFileDir, folderName, logActivity);
                _ = _logger.LogDetailAsync($"{folderName} - Logger setup complete.");
            }
            catch (Exception ex)
            {
                _ = _logger.LogDetailAsync($"Logger setup error: {ex.Message}");
            }

            return _logger;
        }
    }
}
