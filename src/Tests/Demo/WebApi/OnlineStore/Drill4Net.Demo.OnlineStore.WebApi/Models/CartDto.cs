using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Drill4Net.Demo.OnlineStore.WebApi.Models
{
    public class CartDto
    {
        public Guid Id { get; set; }
        public List<CartItemDto> Products { get; set; }
        public double Total { get; set; }
    }
}
