using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonManager
{
    public interface IJsonData
    {
        void SaveFile<T>(T data);
        T LoadFile<T>() where T : new();
    }
}
