using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Drill4Net.Demo.OnlineStore.Bll
{
    public class Product
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Category { get; set; }
        public double Price { get; set; }
        public int Stock { get; set; }
    }
}
