using System;
using System.Linq;
using NeutronData.DataContexts;
using NeutronData.Models.Lookups;
using NeutronData.Repositories;


namespace NeutronMaintenance
{
    public class SizeCodeManager : ISizeCodeManager
    {
        private readonly GenericRepository<SizeCode> _repoSizeCode;

        /// <summary>
        /// Constructor
        /// </summary>
        public SizeCodeManager(Func<NeutronDb> contextFactory)
        {
            if (contextFactory == null) throw new ArgumentNullException(nameof(contextFactory));
            _repoSizeCode = new GenericRepository<SizeCode>(contextFactory);
        }

        /// <summary>
        /// Get a SizeCode by the Name property
        /// </summary>
        /// <param name="name">The name of the <see cref="SizeCode"/></param>
        /// <returns>SizeCode or First SizeCode in Table</returns>
        public SizeCode Get(string name)
        {
            var rec = string.IsNullOrEmpty(name) ? null : _repoSizeCode.All(r => r.Name == name).FirstOrDefault();
            if (rec != null) return rec;
            {
                var seq = _repoSizeCode.All().Select(r => r.Sequence).Max();
                var newRec = new SizeCode { Name = name, Sequence = (seq + 10)};
                _repoSizeCode.Insert(newRec);
                rec = newRec;
            }
            return rec;
        }

        /// <summary>
        /// Gets a SizeCode by the Id property
        /// </summary>
        /// <param name="id">The Id of the <see cref="SizeCode"/></param>
        /// <returns>SizeCode or null</returns>
        public SizeCode Get(int id)
        {
            return _repoSizeCode.All(r => r.Id == id).FirstOrDefault();
        }
    }
}