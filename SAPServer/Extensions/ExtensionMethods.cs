using System;
using System.IO;

namespace SAPServer.Extensions
{
    public static class ExtensionMethods
    {
        public static bool EnsurePathExists(this FileInfo fileInfo)
        {
            var result = false;

            try
            {
                var path = $"{fileInfo.DirectoryName}\\";
                if (!string.IsNullOrEmpty(path))
                {
                    result = Directory.Exists(path);
                }
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }

        public static string GetComputerName(this FileInfo fileInfo)
        {
            var computerName = string.Empty;
            var directoryName = fileInfo.DirectoryName;
            if (directoryName == null || !directoryName.StartsWith($"\\")) return computerName;
            //Take off the first 2 backslashes
            var newPath = directoryName.Substring(2);

            var parts = newPath.Split('\\');
            if (parts.Length > 0)
            {
                computerName = parts[0];
            }

            return computerName;
        }

    }
}
