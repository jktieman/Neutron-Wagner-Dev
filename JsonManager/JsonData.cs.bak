using Newtonsoft.Json;
using System;
using System.Collections;
using System.IO;
using System.Linq;

namespace JsonManager
{
    public class JsonData : IJsonData
    {
        private string _rootDirectory;
        
        public JsonData()
        {
            _rootDirectory = Environment.ExpandEnvironmentVariables(@"%SystemDrive%\NeutronTest\");
           
            if (!Directory.Exists(_rootDirectory))
            {
                _rootDirectory = string.Empty;
                //Directory.CreateDirectory(_rootDirectory);
            }
        }

        public JsonData(string rootDirectory)
        {
            RootDirectory = $"{rootDirectory}";
        }

        public string RootDirectory
        {
            get { return _rootDirectory;}
            set
            {

                if (!string.IsNullOrEmpty(value) && !Directory.Exists(value))
                {
                    Directory.CreateDirectory(value);
                }
                if (!Directory.Exists(JsonDirectory))
                {
                    Directory.CreateDirectory(JsonDirectory);
                }
                _rootDirectory = $"{value}";
            }
        }

        public string JsonDirectory => $"{_rootDirectory}Json\\";

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

            var fileInfo = new FileInfo(JsonDirectory + fileName);

            try
            {
                using (TextWriter writer = new StreamWriter(fileInfo.FullName, append: false))
                {
                    writer.Write(JsonConvert.SerializeObject(data));
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

            var fileInfo = new FileInfo(JsonDirectory + fileName);
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

        public void SaveFile<T>(string file, T data)
        {
            if (string.IsNullOrEmpty(file)) return;
            var fileName = ($"{file}.json");
            var fileInfo = new FileInfo(JsonDirectory + fileName);

            try
            {
                using (TextWriter writer = new StreamWriter(fileInfo.FullName, append: false))
                {
                    writer.Write(JsonConvert.SerializeObject(data));
                }
            }
            catch (Exception)
            {
                Console.Write("Error writing to Json file.");
            }
        }

        public T LoadFile<T>(string file) where T : new()
        {
            var data = new T();
            if (string.IsNullOrEmpty(file)) return data;
            var fileName = ($"{file}.json");
            var fileInfo = new FileInfo(JsonDirectory + fileName);
            if (!fileInfo.Exists) return data;
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
            return data;
        }
    }
}