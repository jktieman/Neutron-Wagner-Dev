using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Neutron.Models
{

    public class MyFileInfo
    {

        private string fileName;
        private string directory;

        public string FileName { get { return fileName; } set { fileName = value; } }

        public string Directory
        {
            get
            {
                if (directory.EndsWith(@"\"))
                {
                    return string.Format("{0}", directory);
                }
                else
                {
                    return string.Format("{0}{1}", directory, @"\");
                }

            }

            set
            {
                directory = value;
            }

        }

        public string FullPath
        {

            get { return string.Format("{0}{1}", this.Directory, this.FileName); }
        }

        public bool Exists()
        {
            bool result = false;

            try
            {
                FileInfo fileInfo = new FileInfo(FullPath);
                var d = fileInfo.Directory.Name;

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
                    string toPath = string.Format("{0}{1}.bak", this.Directory, this.FileName);
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