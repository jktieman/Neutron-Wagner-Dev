using System.Linq;
using NeutronData.DataContexts;
using NeutronData.Models.Lookups;
using NeutronData.Repositories;
using NeutronMaintenance.Models;

namespace NeutronMaintenance
{
    public class HeightCodeManager : IHeightCodeManager
    {
        private readonly GenericRepository<HeightCode> _repoHeightCode = new GenericRepository<HeightCode>(new NeutronDb());

        /// <summary>
        /// Constructor
        /// </summary>
        public HeightCodeManager()
        {
        }

        /// <summary>
        /// Get a HeightCode by the Name property
        /// </summary>
        /// <param name="name">The name of the <see cref="HeightCode"/></param>
        /// <returns>HeightCode or First HeightCode in Table</returns>
        public HeightCode Get(string name)
        {
            var rec = string.IsNullOrEmpty(name) ? null : _repoHeightCode.All().FirstOrDefault(r => r.Name == name);
            if (rec != null) return rec;
            {
                var seq = _repoHeightCode.All().Select(r => r.Sequence).Max();
                var newRec = new HeightCode { Name = name, Sequence = (seq + 10)};
                _repoHeightCode.Insert(newRec);
                rec = newRec;
            }
            return rec;
        }

        /// <summary>
        /// Gets a HeightCode by the Id property
        /// </summary>
        /// <param name="id">The Id of the <see cref="HeightCode"/></param>
        /// <returns>HeightCode or null</returns>
        public HeightCode Get(int id)
        {
            return _repoHeightCode.All().FirstOrDefault(r => r.Id == id);
        }
    }
}