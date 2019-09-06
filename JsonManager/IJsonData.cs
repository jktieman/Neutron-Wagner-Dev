using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonManager
{
    public interface IJsonData
    {
        string RootDirectory { get; set; }

        void SaveFile<T>(T data);
        T LoadFile<T>() where T : new();
        void SaveFile<T>(string file, T data);
        T LoadFile<T>(string file) where T : new();
    }
}
