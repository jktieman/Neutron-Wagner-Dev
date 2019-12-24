using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Neutron.Models
{

    public class MyFileInfo
    {
        private string _directory;

        public string FileName { get; set; }

        public string Directory
        {
            get { return _directory.EndsWith(@"\") ? $"{_directory}" : $"{_directory}{@"\"}"; }
            set { _directory = value; }
        }

        public string FullPath
        {
            get { return $"{this.Directory}{this.FileName}"; }
        }

        public bool Exists()
        {
            var result = false;

            try
            {
                var fileInfo = new FileInfo(FullPath);
                if (fileInfo.Directory != null)
                {
                    var d = fileInfo.Directory.Name;
                }

                result = fileInfo.Exists;
            }
            catch (Exception ex)
            {
                MessageBox.Show("File Exists Error " + ex.Message);
                return false;
            }
            return result;
        }

        public void Backup()
        {
            try
            {
                if (File.Exists(FullPath))
                {
                    var toPath = $"{this.Directory}{this.FileName}.bak";
                    File.Delete(toPath);
                    File.Move(this.FullPath, toPath );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Backup failed " + ex.Message);
            }

        }
    }
}