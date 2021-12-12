using System;

namespace Drill4Net.Demo.OnlineStore.Bll.Contracts.Models
{
    public class CartItem
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal ProductPrice { get; set; }
        public int ProductQuantity { get; set; }
        public decimal TotalPrice
        {
            get
            {
                return ProductPrice * ProductQuantity;
            }
        }
    }
}
