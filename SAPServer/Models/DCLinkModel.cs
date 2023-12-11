using System.Data.Entity;

namespace SAPServer.Models
{
    public class DCLinkModel : DbContext
    {
        public DCLinkModel()
            : base("name=DCLinkModel")
        {
        }

        //public virtual DbSet<OnHand> OnHands { get; set; }
        //public virtual DbSet<NOVA_HISTORY> NOVA_HISTORY { get; set; }
        //public virtual DbSet<NOVA_INPUT> NOVA_INPUT { get; set; }
        //public virtual DbSet<NOVA_OH> NOVA_OH { get; set; }
        //public virtual DbSet<NOVA_OUTPUT> NOVA_OUTPUT { get; set; }

        //protected override void OnModelCreating(DbModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<NOVA_HISTORY>()
        //        .Property(e => e.TRANSID);

        //    modelBuilder.Entity<NOVA_HISTORY>()
        //        .Property(e => e.TRANSTYPE)
        //        .IsFixedLength()
        //        .IsUnicode(false);

        //    modelBuilder.Entity<NOVA_HISTORY>()
        //        .Property(e => e.SKU)
        //        .IsFixedLength()
        //        .IsUnicode(false);

        //    modelBuilder.Entity<NOVA_HISTORY>()
        //        .Property(e => e.QTY)
        //        .HasPrecision(18, 0);

        //    modelBuilder.Entity<NOVA_HISTORY>()
        //        .Property(e => e.FLOCN)
        //        .IsFixedLength()
        //        .IsUnicode(false);

        //    modelBuilder.Entity<NOVA_HISTORY>()
        //        .Property(e => e.TLOCN)
        //        .IsFixedLength()
        //        .IsUnicode(false);

        //    modelBuilder.Entity<NOVA_HISTORY>()
        //        .Property(e => e.BP)
        //        .IsFixedLength()
        //        .IsUnicode(false);

        //    modelBuilder.Entity<NOVA_HISTORY>()
        //        .Property(e => e.ACCOUNTNO)
        //        .IsFixedLength()
        //        .IsUnicode(false);

        //    modelBuilder.Entity<NOVA_HISTORY>()
        //        .Property(e => e.DIVISION)
        //        .IsFixedLength()
        //        .IsUnicode(false);

        //    modelBuilder.Entity<NOVA_HISTORY>()
        //        .Property(e => e.ORDERNO)
        //        .HasPrecision(18, 0);

        //    modelBuilder.Entity<NOVA_HISTORY>()
        //        .Property(e => e.INVOICENO)
        //        .HasPrecision(18, 0);

        //    modelBuilder.Entity<NOVA_HISTORY>()
        //        .Property(e => e.TOTENO)
        //        .HasPrecision(18, 0);

        //    modelBuilder.Entity<NOVA_HISTORY>()
        //        .Property(e => e.PRIORITY)
        //        .IsFixedLength()
        //        .IsUnicode(false);

        //    modelBuilder.Entity<NOVA_HISTORY>()
        //        .Property(e => e.BATCHNO)
        //        .HasPrecision(18, 0);

        //    modelBuilder.Entity<NOVA_HISTORY>()
        //        .Property(e => e.SEQUENCENO)
        //        .HasPrecision(18, 0);

        //    modelBuilder.Entity<NOVA_HISTORY>()
        //        .Property(e => e.TASKNO)
        //        .HasPrecision(18, 0);

        //    modelBuilder.Entity<NOVA_HISTORY>()
        //        .Property(e => e.SKUDESC)
        //        .IsFixedLength()
        //        .IsUnicode(false);

        //    modelBuilder.Entity<NOVA_HISTORY>()
        //        .Property(e => e.USERID)
        //        .IsFixedLength()
        //        .IsUnicode(false);

        //    modelBuilder.Entity<NOVA_HISTORY>()
        //        .Property(e => e.UPC)
        //        .IsFixedLength()
        //        .IsUnicode(false);

        //    modelBuilder.Entity<NOVA_HISTORY>()
        //        .Property(e => e.ACTION)
        //        .IsFixedLength()
        //        .IsUnicode(false);

        //    modelBuilder.Entity<NOVA_HISTORY>()
        //        .Property(e => e.CYCLECOUNTNO)
        //        .HasPrecision(18, 0);

        //    modelBuilder.Entity<NOVA_HISTORY>()
        //        .Property(e => e.REASONCODE)
        //        .IsFixedLength()
        //        .IsUnicode(false);

        //    modelBuilder.Entity<NOVA_HISTORY>()
        //        .Property(e => e.PROCESSED)
        //        .IsFixedLength()
        //        .IsUnicode(false);

        //    modelBuilder.Entity<NOVA_HISTORY>()
        //        .Property(e => e.EXPLANATION)
        //        .IsFixedLength()
        //        .IsUnicode(false);

        //    modelBuilder.Entity<NOVA_HISTORY>()
        //        .Property(e => e.FULLFLAG)
        //        .IsFixedLength()
        //        .IsUnicode(false);

        //    modelBuilder.Entity<NOVA_HISTORY>()
        //        .Property(e => e.BEGINNINGQTY)
        //        .HasPrecision(18, 0);

        //    modelBuilder.Entity<NOVA_HISTORY>()
        //        .Property(e => e.ID)
        //        .HasPrecision(18, 0);

        //    modelBuilder.Entity<NOVA_INPUT>()
        //        .Property(e => e.TRANSID);

        //    modelBuilder.Entity<NOVA_INPUT>()
        //        .Property(e => e.TRANSTYPE)
        //        .IsFixedLength()
        //        .IsUnicode(false);

        //    modelBuilder.Entity<NOVA_INPUT>()
        //        .Property(e => e.SKU)
        //        .IsFixedLength()
        //        .IsUnicode(false);

        //    modelBuilder.Entity<NOVA_INPUT>()
        //        .Property(e => e.QTY)
        //        .HasPrecision(18, 0);

        //    modelBuilder.Entity<NOVA_INPUT>()
        //        .Property(e => e.FLOCN)
        //        .IsFixedLength()
        //        .IsUnicode(false);

        //    modelBuilder.Entity<NOVA_INPUT>()
        //        .Property(e => e.TLOCN)
        //        .IsFixedLength()
        //        .IsUnicode(false);

        //    modelBuilder.Entity<NOVA_INPUT>()
        //        .Property(e => e.BP)
        //        .IsFixedLength()
        //        .IsUnicode(false);

        //    modelBuilder.Entity<NOVA_INPUT>()
        //        .Property(e => e.ACCOUNTNO)
        //        .IsFixedLength()
        //        .IsUnicode(false);

        //    modelBuilder.Entity<NOVA_INPUT>()
        //        .Property(e => e.DIVISION)
        //        .IsFixedLength()
        //        .IsUnicode(false);

        //    modelBuilder.Entity<NOVA_INPUT>()
        //        .Property(e => e.ORDERNO)
        //        .HasPrecision(18, 0);

        //    modelBuilder.Entity<NOVA_INPUT>()
        //        .Property(e => e.INVOICENO)
        //        .HasPrecision(18, 0);

        //    modelBuilder.Entity<NOVA_INPUT>()
        //        .Property(e => e.PRIORITY)
        //        .IsFixedLength()
        //        .IsUnicode(false);

        //    modelBuilder.Entity<NOVA_INPUT>()
        //        .Property(e => e.TOTENO)
        //        .HasPrecision(18, 0);

        //    modelBuilder.Entity<NOVA_INPUT>()
        //        .Property(e => e.BATCHNO)
        //        .HasPrecision(18, 0);

        //    modelBuilder.Entity<NOVA_INPUT>()
        //        .Property(e => e.SEQUENCENO)
        //        .HasPrecision(18, 0);

        //    modelBuilder.Entity<NOVA_INPUT>()
        //        .Property(e => e.TASKNO)
        //        .HasPrecision(18, 0);

        //    modelBuilder.Entity<NOVA_INPUT>()
        //        .Property(e => e.SKUDESC)
        //        .IsFixedLength()
        //        .IsUnicode(false);

        //    modelBuilder.Entity<NOVA_INPUT>()
        //        .Property(e => e.USERID)
        //        .IsFixedLength()
        //        .IsUnicode(false);

        //    modelBuilder.Entity<NOVA_INPUT>()
        //        .Property(e => e.UPC)
        //        .IsFixedLength()
        //        .IsUnicode(false);

        //    modelBuilder.Entity<NOVA_INPUT>()
        //        .Property(e => e.ACTION)
        //        .IsFixedLength()
        //        .IsUnicode(false);

        //    modelBuilder.Entity<NOVA_INPUT>()
        //        .Property(e => e.PROCESSED)
        //        .IsFixedLength()
        //        .IsUnicode(false);

        //    modelBuilder.Entity<NOVA_OH>()
        //        .Property(e => e.sku)
        //        .IsFixedLength()
        //        .IsUnicode(false);

        //    modelBuilder.Entity<NOVA_OH>()
        //        .Property(e => e.loc)
        //        .IsFixedLength()
        //        .IsUnicode(false);

        //    modelBuilder.Entity<NOVA_OH>()
        //        .Property(e => e.qty)
        //        .HasPrecision(9, 0);

        //    modelBuilder.Entity<NOVA_OUTPUT>()
        //        .Property(e => e.TRANSID);

        //    modelBuilder.Entity<NOVA_OUTPUT>()
        //        .Property(e => e.TRANSTYPE)
        //        .IsFixedLength()
        //        .IsUnicode(false);

        //    modelBuilder.Entity<NOVA_OUTPUT>()
        //        .Property(e => e.SKU)
        //        .IsFixedLength()
        //        .IsUnicode(false);

        //    modelBuilder.Entity<NOVA_OUTPUT>()
        //        .Property(e => e.QTY)
        //        .HasPrecision(18, 0);

        //    modelBuilder.Entity<NOVA_OUTPUT>()
        //        .Property(e => e.FLOCN)
        //        .IsFixedLength()
        //        .IsUnicode(false);

        //    modelBuilder.Entity<NOVA_OUTPUT>()
        //        .Property(e => e.TLOCN)
        //        .IsFixedLength()
        //        .IsUnicode(false);

        //    modelBuilder.Entity<NOVA_OUTPUT>()
        //        .Property(e => e.BP)
        //        .IsFixedLength()
        //        .IsUnicode(false);

        //    modelBuilder.Entity<NOVA_OUTPUT>()
        //        .Property(e => e.ACCOUNTNO)
        //        .IsFixedLength()
        //        .IsUnicode(false);

        //    modelBuilder.Entity<NOVA_OUTPUT>()
        //        .Property(e => e.DIVISION)
        //        .IsFixedLength()
        //        .IsUnicode(false);

        //    modelBuilder.Entity<NOVA_OUTPUT>()
        //        .Property(e => e.ORDERNO)
        //        .HasPrecision(18, 0);

        //    modelBuilder.Entity<NOVA_OUTPUT>()
        //        .Property(e => e.INVOICENO)
        //        .HasPrecision(18, 0);

        //    modelBuilder.Entity<NOVA_OUTPUT>()
        //        .Property(e => e.PRIORITY)
        //        .IsFixedLength()
        //        .IsUnicode(false);

        //    modelBuilder.Entity<NOVA_OUTPUT>()
        //        .Property(e => e.TOTENO)
        //        .HasPrecision(18, 0);

        //    modelBuilder.Entity<NOVA_OUTPUT>()
        //        .Property(e => e.BATCHNO)
        //        .HasPrecision(18, 0);

        //    modelBuilder.Entity<NOVA_OUTPUT>()
        //        .Property(e => e.SEQUENCENO)
        //        .HasPrecision(18, 0);

        //    modelBuilder.Entity<NOVA_OUTPUT>()
        //        .Property(e => e.TASKNO)
        //        .HasPrecision(18, 0);

        //    modelBuilder.Entity<NOVA_OUTPUT>()
        //        .Property(e => e.SKUDESC)
        //        .IsFixedLength()
        //        .IsUnicode(false);

        //    modelBuilder.Entity<NOVA_OUTPUT>()
        //        .Property(e => e.USERID)
        //        .IsFixedLength()
        //        .IsUnicode(false);

        //    modelBuilder.Entity<NOVA_OUTPUT>()
        //        .Property(e => e.UPC)
        //        .IsFixedLength()
        //        .IsUnicode(false);

        //    modelBuilder.Entity<NOVA_OUTPUT>()
        //        .Property(e => e.CYCLECOUNTNO)
        //        .HasPrecision(18, 0);

        //    modelBuilder.Entity<NOVA_OUTPUT>()
        //        .Property(e => e.REASONCODE)
        //        .IsFixedLength()
        //        .IsUnicode(false);

        //    modelBuilder.Entity<NOVA_OUTPUT>()
        //        .Property(e => e.PROCESSED)
        //        .IsFixedLength()
        //        .IsUnicode(false);

        //    modelBuilder.Entity<NOVA_OUTPUT>()
        //        .Property(e => e.EXPLANATION)
        //        .IsFixedLength()
        //        .IsUnicode(false);

        //    modelBuilder.Entity<NOVA_OUTPUT>()
        //        .Property(e => e.FULLFLAG)
        //        .IsFixedLength()
        //        .IsUnicode(false);

        //    modelBuilder.Entity<NOVA_OUTPUT>()
        //        .Property(e => e.BEGINNINGQTY)
        //        .HasPrecision(18, 0);

        //    modelBuilder.Entity<NOVA_OUTPUT>()
        //        .Property(e => e.SUBLEDGER)
        //        .IsFixedLength()
        //        .IsUnicode(false);

        //    modelBuilder.Entity<NOVA_OUTPUT>()
        //        .Property(e => e.TYPE)
        //        .IsFixedLength()
        //        .IsUnicode(false);
        //}
    }
}
