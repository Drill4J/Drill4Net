using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drill4Net.Demo.OnlineStore.Dal
{
    public static class DataContext
    {
        public static List <Product> Products { get; set; }
        public static List <Cart> Carts { get; set; }

        static DataContext()
        {
            Products = new List<Product>();
            Carts = new List<Cart>();
            Products.AddRange(new List<Product>(){new Product
            {
                Id = new Guid(),
                Name = "Product1",
                Category = "SeaFood",
                Price = 10.50
            } ,
            new Product
            {
                Id = new Guid(),
                Name = "Product2",
                Category = "SeaFood",
                Price = 7.60
            }});

        }
    }
}
