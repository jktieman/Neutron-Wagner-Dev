using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeutronData.DataContexts;
using NeutronData.Models;

namespace NeutronData.Repositories
{
    public class SerialConfigurationsRepository
    {
        private readonly GenericRepository<SerialConfiguration> _repo = new GenericRepository<SerialConfiguration>(new NeutronDb());

        public SerialConfigurationsRepository()
        {

        }
    }
}
