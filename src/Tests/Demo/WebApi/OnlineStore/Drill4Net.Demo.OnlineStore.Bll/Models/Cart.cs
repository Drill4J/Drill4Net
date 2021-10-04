using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Drill4Net.Demo.OnlineStore.Bll.Models
{
    public class Cart
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public List<CartItem> Products { get; set; } = new List<CartItem>();
        public decimal Total { 
            get 
            {
                return Products.Sum(p => p.TotalPrice);
            }
        }
    }
}
