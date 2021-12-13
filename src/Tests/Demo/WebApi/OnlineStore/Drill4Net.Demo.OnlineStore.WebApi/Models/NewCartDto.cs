using System.Collections.Generic;


namespace Drill4Net.Demo.OnlineStore.WebApi.Models
{
    public class NewCartDto
    {
        public List<CartItemDto> Products { get; set; }
        public decimal Total { get; set; }
    }
}
