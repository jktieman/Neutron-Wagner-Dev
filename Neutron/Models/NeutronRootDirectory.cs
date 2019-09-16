using System;
using System.IO;
using Neutron.Interfaces;

namespace Neutron.Models
{
    public class NeutronRootDirectory : INeutronRootDirectory
    {
        private string _rootDirectory;

        public NeutronRootDirectory()
        {
            SetRootDirectory();
        }

        public string RootDirectory
        {
            get { return _rootDirectory; }
            set { _rootDirectory = value; }
        }

        public string SetRootDirectory()
        {
            var alternateRootDirectory = Environment.ExpandEnvironmentVariables(@"%SystemDrive%\Neutron\");
            _rootDirectory = !Directory.Exists(alternateRootDirectory) ? string.Empty : alternateRootDirectory;
            return _rootDirectory;
        }
    }
}
