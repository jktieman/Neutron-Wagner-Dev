using System;
using System.IO;
using System.Linq;

namespace Core.Common
{
    public static class BackupFile
    {
        public static string GetBackupFileName(string archiveDir, FileInfo fileInfo)
        {
            string backupFileName = string.Empty;
            int curSeq = -1;
            int startPosition = -1;
            var di = new DirectoryInfo(archiveDir);
            var fileName = Path.GetFileNameWithoutExtension(fileInfo.Name);
            var searchFileName = string.Format("{0}*.*", fileName);
            var ext = Path.GetExtension(fileInfo.Name);
            if (di.Exists)
            {
                var files = di.GetFiles(searchFileName);
                foreach (var f in files)
                {
                    var localFileInfo = f;
                    var dashLocation = localFileInfo.Name.LastIndexOf('_');
                    if (dashLocation > 0)
                    {
                        startPosition = dashLocation + 1;
                        var dotLocation = localFileInfo.Name.LastIndexOf('.');
                        if (dotLocation > startPosition)
                        {
                            int count = dotLocation - startPosition;
                            var result = localFileInfo.Name.Substring(startPosition, count);
                            var num = -1;
                            var t = int.TryParse(result, out num);
                            if (t)
                            {
                                if (num > curSeq)
                                {
                                    curSeq = num;
                                }
                            }
                        }
                    }
                }
                if (curSeq == -1)
                {
                    curSeq = 1;
                }
                else
                {
                    curSeq += 1;
                }
                var strCurSeq = string.Format(@"{0}{1}", '_', curSeq.ToString());

                backupFileName = string.Format(@"{0}{1}{2}{3}", archiveDir, fileName, strCurSeq, ext);
                return backupFileName;
            }
            else
            {
                return backupFileName;
            }
        }

    }
}
