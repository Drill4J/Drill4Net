using System;
using System.Collections.Generic;

namespace Drill4Net.Demo.OnlineStore.WebApi.Models
{
    public class CartDto
    {
        public Guid Id { get; set; }
        public List<CartItemDto> Products { get; set; }
        public decimal Total { get; set; }
    }
}
