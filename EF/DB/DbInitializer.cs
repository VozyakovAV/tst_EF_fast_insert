using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF
{
    public class DbInitializer : DropCreateDatabaseIfModelChanges<MyContext>
    //public class DbInitializer : DropCreateDatabaseAlways<MyContext>
    {
    }
}
