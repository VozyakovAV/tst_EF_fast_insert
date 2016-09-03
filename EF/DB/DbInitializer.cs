using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF
{
    public class DbInitializer : CreateDatabaseIfNotExists<MyContext>
    {
        protected override void Seed(MyContext db)
        {
            db.Items.Add(new Item { Name = "Война и мир" });
            db.Items.Add(new Item { Name = "гы гы гы" });

            base.Seed(db);
        }
    }
}
