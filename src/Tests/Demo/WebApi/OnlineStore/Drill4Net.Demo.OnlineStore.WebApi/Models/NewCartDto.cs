using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Drill4Net.Demo.OnlineStore.WebApi.Models
{
    public class NewCartDto
    {
        public List<CartItemDto> Products { get; set; }
        public decimal Total { get; set; }
    }
}
