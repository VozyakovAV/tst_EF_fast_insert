using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF
{
    public class MyContext : DbContext
    {
        public DbSet<Item> Items { get; set; }
    }


}
