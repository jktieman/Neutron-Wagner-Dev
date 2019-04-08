using NeutronData.DataContexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutronData.Repositories
{
    public class NeutronSizeAndVelClassRepository : RepositoryBase<NeutronDb>
    {
        public string GetSizeString(byte b)
        {
            var s = DataContext.SizeClassesBToS.Find(b).StringSize;
            if (string.IsNullOrEmpty(s))
            {
                return "Z";
            }
            return s;
        }

        public string GetVelString(byte v)
        {
            var s = DataContext.VelClassesBToS.Find(v).StringSize;
            if (string.IsNullOrEmpty(s))
            {
                return "Z";
            }
            return s;
        }
    }
}
