using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF
{
    public class Maps : EntityTypeConfiguration<SubItem>
    {
        public Maps()
        {
            HasRequired(t => t.Item)
                .WithMany(p => p.SubItems)
                .HasForeignKey(t => t.ItemId)
                .WillCascadeOnDelete(true);
        }
    }
}
