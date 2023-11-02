using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AlliedLogger
{
    public interface IDynamicLogger
    {
        string LogActivity { get; set; }
        string LogFileDir { get; set; }
        string FolderName { get; set; }
        string FileName { get; set; }
        string FilePath { get; }
        string TempFilePath { get; }
        void Flush();
        void Log(string msg);
        Task LogAsync(string msg);

        void LogDetail(string msg = ""
            , [CallerMemberName] string origin = ""
            , [CallerFilePath] string filePath = ""
            , [CallerLineNumber] int lineNumber = 0);

        Task LogDetailAsync(string msg = ""
            , [CallerMemberName] string origin = ""
            , [CallerFilePath] string filePath = ""
            , [CallerLineNumber] int lineNumber = 0);

       // List<string> LastLogLines(int numLines = 10);
       StringBuilder LastLogLines(int lines = 10);
    }
}