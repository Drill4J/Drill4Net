using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Drill4Net.Demo.OnlineStore.Bll
{
    public class Cart
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public List<Product> Products { get; set; } = new List<Product>();
        public double Total { 
            get 
            {
                double total = 0;
                foreach(var product in Products)
                {
                    total += product.Price * product.Stock;
                }
                return total;
            }
        }
    }
}
