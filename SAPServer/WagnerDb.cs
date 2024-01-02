using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using NeutronData.Models;
using SAPServer.Models;
using NeutronData.Models;

namespace SAPServer
{
    public class WagnerDb : DbContext
    {
        //public WagnerDb() : base("name=DCLinkModel")
        public WagnerDb() : base("name=Neutron")
        {
            Database.SetInitializer(new NullDatabaseInitializer<WagnerDb>());
        }

        public override int SaveChanges()
        {
            try
            {
                return base.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                var errorMessages = ex.EntityValidationErrors
                        .SelectMany(x => x.ValidationErrors)
                        .Select(x => x.ErrorMessage);

                var fullErrorMessage = string.Join("; ", errorMessages);

                var exceptionMessage = string.Concat(ex.Message, " The validation errors are: ", fullErrorMessage);

                throw new DbEntityValidationException(exceptionMessage, ex.EntityValidationErrors);
            }
        }

        public virtual DbSet<OnHand> OnHands { get; set; }
        //public DbSet<NOVA_HISTORY> NOVA_HISTORY { get; set; }
        //public DbSet<NOVA_INPUT> NOVA_INPUT { get; set; }
        //public DbSet<NOVA_OH> NOVA_OH { get; set; }
        //public DbSet<NOVA_OUTPUT> NOVA_OUTPUT { get; set; }
        //public DbSet<PriorityRecord> PriorityRecords { get; set; }

    }
}
