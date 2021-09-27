using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Drill4Net.Demo.OnlineStore.Bll
{
    public class CartDto
    {
        public Guid Id { get; set; }
        public List<Product> Products { get; set; }
        public double Total { get; set; }
    }
}
