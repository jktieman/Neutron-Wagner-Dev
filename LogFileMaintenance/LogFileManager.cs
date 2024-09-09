using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AlliedLogger;
using AsyncAwaitBestPractices;
using Timer = System.Timers.Timer;

namespace LogFileMaintenance
{
    public class LogFileManager
    {
        private readonly string _logFileFolder;
        private readonly uint _maximumAgeInDays;
        private readonly IDynamicLogger _logger;
        private Timer _timer;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public LogFileManager(string logFileFolder, uint maximumAgeInDays, IDynamicLogger logger)
        {
            _logFileFolder = logFileFolder;
            _maximumAgeInDays = maximumAgeInDays;
            _logger = logger;
        }

        public void StartLogFileManagementService()
        {
            var interval = TimeSpan.FromHours(8);
            _timer = new Timer(interval.TotalMilliseconds);
            _timer.Elapsed += async (sender, e) =>
            {
                if (_semaphore.CurrentCount == 0)
                {
                    return;
                }
                await _semaphore.WaitAsync();
                try
                {
                    await Process();
                }
                finally
                {
                    _semaphore.Release();
                }
            };

            _timer.AutoReset = true;
            _timer.Enabled = true;
            _timer.Start();
        }


        public async Task Process()
        {
            var allFolders = GetSubdirectories(_logFileFolder);

            _logger.LogDetailAsync($"{allFolders.Count} Subdirectories in the specified folder:").SafeFireAndForget();
            foreach (var folder in allFolders)
            {
              await DeleteOldFiles(folder, _maximumAgeInDays);
            }

            _logger.LogDetailAsync("File cleanup completed.").SafeFireAndForget();
        }

        private Task DeleteOldFiles(string folderPath, uint maximumAgeInDays)
        {
            var minimumDate = DateTime.Now.AddDays(-maximumAgeInDays);
            // set the minimumDate Time to midnight
            minimumDate = minimumDate.Date;
            
            var filesToDelete = Directory.EnumerateFiles(folderPath);

            foreach (var file in filesToDelete)
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.LastWriteTime < minimumDate)
                {
                    try
                    {
                        File.Delete(file);
                        _logger.LogDetailAsync($"Deleting: {folderPath} File: {file}").SafeFireAndForget();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDetailAsync($"Error deleting file {file}: {ex.Message}").SafeFireAndForget();
                    }
                }
            }

            return Task.CompletedTask;
        }
        private List<string> GetSubdirectories(string folderPath)
        {
            var subdirectories = new List<string>();
            try
            {
                subdirectories.AddRange(Directory.GetDirectories(folderPath, "*", SearchOption.AllDirectories));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogDetailAsync($"Error accessing directories: {ex.Message}").SafeFireAndForget();
            }
            return subdirectories;
        }
    }
}

