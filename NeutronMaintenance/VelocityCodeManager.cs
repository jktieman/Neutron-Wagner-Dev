using System;
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
        public VelocityCodeManager(Func<NeutronDb> contextFactory)
        {
            if (contextFactory == null) throw new ArgumentNullException(nameof(contextFactory));
            _repoVelocityCode = new GenericRepository<VelocityCode>(contextFactory);
        }

        /// <summary>
        /// Get a VelocityCode by the Name property
        /// </summary>
        /// <param name="name">The name of the <see cref="VelocityCode"/></param>
        /// <returns>VelocityCode or First VelocityCode in Table</returns>
        public VelocityCode Get(string name)
        {
            var rec = string.IsNullOrEmpty(name) ? null : _repoVelocityCode.All(r => r.Name == name).FirstOrDefault();
            if (rec != null) return rec;
            {
                var seq = _repoVelocityCode.All().Select(r => r.Sequence).Max();
                var newRec = new VelocityCode { Name = name, Sequence = (seq + 10)};
                _repoVelocityCode.Insert(newRec);
                rec = newRec;
            }
            return rec;
        }

        /// <summary>
        /// Gets a VelocityCode by the Id property
        /// </summary>
        /// <param name="id">The Id of the <see cref="VelocityCode"/></param>
        /// <returns>VelocityCode or null</returns>
        public VelocityCode Get(int id)
        {
            return _repoVelocityCode.All(r => r.Id == id).FirstOrDefault();
        }
    }
}