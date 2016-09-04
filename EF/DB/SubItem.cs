using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF
{
    public class SubItem
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }

        public int ItemId { get; set; }
        public Item Item { get; set; }
    }
}
