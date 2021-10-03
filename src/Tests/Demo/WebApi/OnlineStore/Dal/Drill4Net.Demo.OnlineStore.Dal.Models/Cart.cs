using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Drill4Net.Demo.OnlineStore.Dal.Models
{
    public class Cart
    {
        public Guid Id { get; set; }
        public List<Product> Products { get; set; }
        public double Total { get; set; }
    }
}
