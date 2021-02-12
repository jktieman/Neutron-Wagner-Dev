using System;
using System.IO;
using System.Security;

namespace NeutronCore.Extensions
{
    public static class FileInfoExtensions
    {
        public static OperationResult IsFileLocked(this FileInfo fileInfo)
        {
            var op = new OperationResult();
            op.Success = false;

            FileStream stream = null;
            try
            {
                stream = fileInfo.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }

            catch (SecurityException se)
            {
                op.AddMessage(fileInfo.FullName + Environment.NewLine + "Security Exception Error"
                    + Environment.NewLine + se.Message);
                op.Success = true;
                return op;
            }
            catch (FileNotFoundException fe)
            {
                op.AddMessage(fileInfo.FullName + Environment.NewLine + "File Not Found Exception Error"
                    + Environment.NewLine + fe.Message);
                op.Success = true;

                File.CreateText(fileInfo.FullName);
                op.AddMessage(fileInfo.FullName + " CREATED.");
                op.Success = false;
                return op;
            }
            catch (ArgumentNullException ae)
            {
                op.AddMessage(fileInfo.FullName + Environment.NewLine + "Argument Null Exception Error"
                    + Environment.NewLine + ae.Message);
                op.Success = true;
                return op;
            }
            catch (UnauthorizedAccessException ue)
            {
                op.AddMessage(fileInfo.FullName + Environment.NewLine + "Unauthorized Access Exception Error"
                    + Environment.NewLine + ue.Message);
                op.Success = true;
                return op;
            }
            catch (DirectoryNotFoundException de)
            {
                op.AddMessage(fileInfo.FullName + Environment.NewLine + "Directory Not Found Exception Error"
                    + Environment.NewLine + de.Message);
                op.Success = true;
                return op;
            }
            catch (IOException ie)
            {
                op.AddMessage(fileInfo.FullName);
                op.AddMessage(message: "IO Exception Error");
                op.AddMessage(ie.Message);
                op.Success = true;
                return op;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }
            return op;
        }

        public static OperationResult FileLockFailure(this FileInfo fileInfo)
        {
            var op = new OperationResult();
            op.Success = false;
            var waitTime = 100;
            var maxWaitTime = 500;

            while (fileInfo.IsFileLocked().Success)
            {
                var msg = fileInfo.IsFileLocked().Message();
                op.AddMessage(fileInfo.Name);
                op.AddMessage(message: "File Created - File is LOCKED:");
                op.AddMessage(msg);
                System.Threading.Thread.Sleep(waitTime);
                waitTime += 100;
                if (waitTime >= maxWaitTime)
                {
                    op.Success = true;
                    op.AddMessage(fileInfo.Name);
                    op.AddMessage(message: "Error - File Created - File Lock Failure:");
                    op.AddMessage(msg);
                    break;
                }
            }
            return op;
        }
    }
}

