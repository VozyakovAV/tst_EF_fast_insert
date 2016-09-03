using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF
{
    class Program
    {
        static void Main(string[] args)
        {
            Database.SetInitializer(new DbInitializer());
            using (var ctx = new MyContext())
            {
                ctx.Configuration.AutoDetectChangesEnabled = false;
                //ctx.Configuration.ValidateOnSaveEnabled = false;

                var list = new List<Item>();
                for (int i = 0; i < 1000; i++)
                {
                    list.Add(new Item() { Number = i, Name = i.ToString() });
                }
                
                var sw = Stopwatch.StartNew();
                ctx.Items.AddRange(list);
                ctx.SaveChanges();
                sw.Stop();
                Console.WriteLine(sw.Elapsed);
            }
            Console.ReadKey();
        }
    }
}
