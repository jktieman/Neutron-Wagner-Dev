using AlliedLogger;
using System;
using System.IO;

namespace NeutronCore
{
    public static class ArchiveFile
    {
        public static void Archive(FileInfo fileInfo)
        {
            string archiveDir = string.Format(@"{0}Archive\{1}", LoaderSettings.GetHostOrderDirectory(), fileInfo.Name);
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
                Logger.Log($"Archive File Error:  {ex.Message} \r\n  {ex.InnerException}");
            }
        }
    }
}
