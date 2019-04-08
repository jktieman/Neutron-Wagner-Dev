using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonManager
{
    public class JsonData : IJsonData
    {
        readonly string rootDirectory;

        public JsonData()
        {
            rootDirectory = Environment.ExpandEnvironmentVariables(@"%SystemDrive%\Neutron\Json\");
            if (!Directory.Exists(rootDirectory))
            {
                Directory.CreateDirectory(rootDirectory);
            }
        }

        public void SaveFile<T>(T data)
        {
            string fileName;

            if (data is IList)  //its a list
            {
                Type type = data.GetType().GetGenericArguments().Single();

                fileName = ($"{type.Name}s.json");
            }
            else
            {
                fileName = ($"{typeof(T).Name}.json"); //single class
            }

            var fileInfo = new FileInfo(rootDirectory + fileName);

            try
            {
                using (TextWriter writer = new StreamWriter(fileInfo.FullName, append: false))
                {
                    writer.Write(Newtonsoft.Json.JsonConvert.SerializeObject(data));
                }
            }
            catch (Exception)
            {
                Console.Write("Error writing to Json file.");
            }
        }

        public T LoadFile<T>() where T : new()
        {
            var data = new T();
            string fileName;

            if (data is IList)  //its a list
            {
                Type type = data.GetType().GetGenericArguments().Single();

                fileName = ($"{type.Name}s.json");
            }
            else
            {
                fileName = ($"{typeof(T).Name}.json"); //single class
            }

            var fileInfo = new FileInfo(rootDirectory + fileName);
            if (fileInfo.Exists)
            {
                try
                {
                    using (TextReader reader = new StreamReader(fileInfo.FullName))
                    {
                        data = JsonConvert.DeserializeObject<T>(reader.ReadToEnd());
                    }
                }
                catch (Exception)
                {
                    Console.Write($"Error reading from Json file.  {fileInfo.FullName}");
                }
            }
            return data;
        }
    }
}