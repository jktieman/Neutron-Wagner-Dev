using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostLoader
{
    public class HostPreferences
    {
        private string hostFolder;
        private string filter;
        private bool includeSubdirectories;
        private string novahostFolder;
        private string newItemFileName;

        public string NewItemFileName
        {
            get { return newItemFileName; }
            set { newItemFileName = value; }
        }

        public string NovaHostFolder
        {
            get { return novahostFolder; }
            set { novahostFolder = value; }
        }


        public bool IncludeSubdirectories
        {
            get { return includeSubdirectories; }
            set { includeSubdirectories = value; }
        }

        public string Filter
        {
            get { return filter; }
            set { filter = value; }
        }

        public string HostFolder
        {
            get { return hostFolder; }
            set { hostFolder = value; }
        }

    }
}
