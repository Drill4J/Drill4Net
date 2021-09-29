using System;
using System.Collections.Generic;

namespace Drill4Net.Demo.OnlineStore.Dal
{
    public static class DataContext
    {
        internal static List <Product> Products { get; set; }
        internal static List <Cart> Carts { get; set; }

        static DataContext()
        {
            Products = new List<Product>();
            Carts = new List<Cart>();
            Products.AddRange(new List<Product>(){
                new Product
                    {
                        Id = Guid.NewGuid(),
                        Name = "Product1",
                        Category = "SeaFood",
                        Price = 10.50,
                        Stock=5
                    } ,
                new Product
                    {
                        Id = Guid.NewGuid(),
                        Name = "Product3",
                        Category = "SeaFood",
                        Price = 11.10,
                        Stock=2
                    },
                new Product
                    {
                        Id = Guid.NewGuid(),
                        Name = "Product2",
                        Category = "SeaFood",
                        Price = 7.60,
                        Stock=12
                    }});
        }
    }
}
