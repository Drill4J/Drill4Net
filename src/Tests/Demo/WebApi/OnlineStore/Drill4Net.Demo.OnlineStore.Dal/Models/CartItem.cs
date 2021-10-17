using System;

namespace Drill4Net.Demo.OnlineStore.Dal.Models
{
    public class CartItem
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal ProductPrice { get; set; }
        public decimal ProductQuantity { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
