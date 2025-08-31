using System;
using System.IO;
using Neutron.Interfaces;

namespace Neutron.Models
{
    public class NeutronRootDirectory : INeutronRootDirectory
    {
        public string RootDirectory { get; set; }

        public NeutronRootDirectory()
        {
            SetRootDirectory();
        }
        
        public string SetRootDirectory()
        {

            var alternateRootDirectory = Environment.ExpandEnvironmentVariables(@"%SystemDrive%\NeutronTest\");

            RootDirectory = !Directory.Exists(alternateRootDirectory) ? string.Empty : alternateRootDirectory;
            return RootDirectory;
        }
    }
}
