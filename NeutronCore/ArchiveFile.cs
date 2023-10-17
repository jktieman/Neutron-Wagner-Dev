using AlliedLogger;
using System;
using System.IO;

namespace NeutronCore
{
    public static class ArchiveFile
    {
        public static void Archive(FileInfo fileInfo, IDynamicLogger logger)
        {
            var archiveDir = $@"{LoaderSettings.GetHostOrderDirectory()}Archive\{fileInfo.Name}";
            try
            {
                if (File.Exists(fileInfo.FullName))
                {
                    if (File.Exists(archiveDir))
                    {
                        File.Delete(archiveDir);
                    }

                    fileInfo.MoveTo(archiveDir);
                }
            }
            catch (Exception ex)
            {
                logger.LogDetailAsync($"Archive File Error:  {ex.Message}{Environment.NewLine}{ex.InnerException}");
            }
        }
    }
}
