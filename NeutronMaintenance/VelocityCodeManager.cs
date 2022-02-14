using System.Linq;
using NeutronData.DataContexts;
using NeutronData.Models.Lookups;
using NeutronData.Repositories;
using NeutronMaintenance.Models;

namespace NeutronMaintenance
{
    public class VelocityCodeManager : IVelocityCodeManager
    {
        private readonly GenericRepository<VelocityCode> _repoVelocityCode;

        /// <summary>
        /// Constructor
        /// </summary>
        public VelocityCodeManager()
        {
            _repoVelocityCode = new GenericRepository<VelocityCode>(new NeutronDb());
        }

        /// <summary>
        /// Get a VelocityCode by the Name property
        /// </summary>
        /// <param name="name">The name of the <see cref="VelocityCode"/></param>
        /// <returns>VelocityCode or null</returns>
        public VelocityCode Get(string name)
        {
            var rec = string.IsNullOrEmpty(name) ? null : _repoVelocityCode.All().FirstOrDefault(r => r.Name == name);
            if (rec == null)
            {

                // return the default VelocityCode
            }
            return null;
        }

        /// <summary>
        /// Gets a VelocityCode by the Id property
        /// </summary>
        /// <param name="id">The Id of the <see cref="VelocityCode"/></param>
        /// <returns>VelocityCode or null</returns>
        public VelocityCode Get(int id)
        {
            return _repoVelocityCode.All().FirstOrDefault(r => r.Id == id);
        }
    }
}