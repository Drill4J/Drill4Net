using System;
using System.Linq;
using System.Collections.Generic;

namespace Drill4Net.Demo.OnlineStore.Bll.Contracts.Models
{
    public class Cart
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public List<CartItem> Products { get; set; } = new List<CartItem>();
        public decimal Total { 
            get 
            {
                return Products.Sum(p => p!=null?p.TotalPrice:0);
            }
        }
    }
}
