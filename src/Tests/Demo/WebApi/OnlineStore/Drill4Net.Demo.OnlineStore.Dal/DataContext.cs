using Drill4Net.Demo.OnlineStore.Dal.Models;
using System;
using System.Collections.Generic;

namespace Drill4Net.Demo.OnlineStore.Dal
{
    public static class DataContext
    {
        internal static List <Product> Products { get; set; }
        internal static List <Cart> Carts { get; set; }
        internal static List<Cart> CartItems { get; set; }

        static DataContext()
        {
            Products = new List<Product>();
            Carts = new List<Cart>();
            Products.AddRange(new List<Product>(){
                new Product
                    {
                        Id = Guid.NewGuid(),
                        Name = "Product1",
                        Category = "Drinks",
                        Price = 10.50M,
                        Stock=5
                    } ,
                new Product
                    {
                        Id = Guid.NewGuid(),
                        Name = "Product3",
                        Category = "SeaFood",
                        Price = 11.10M,
                        Stock=2
                    },
                new Product
                    {
                        Id = Guid.NewGuid(),
                        Name = "Product37",
                        Category = "Drinks",
                        Price = 18.10M,
                        Stock=2
                    },
                new Product
                    {
                        Id = Guid.NewGuid(),
                        Name = "Product2",
                        Category = "SeaFood",
                        Price = 9.14M,
                        Stock=8
                    },
                new Product
                    {
                        Id = Guid.NewGuid(),
                        Name = "Product32",
                        Category = "SeaFood",
                        Price = 7.60M,
                        Stock=12
                    }});
            Carts.Add(new Cart
            {
                Id = new Guid("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                Products = new List<CartItem>()
                {
                    new CartItem
                    {
                        ProductId=Products[0].Id,
                        ProductName=Products[0].Name,
                        ProductPrice=Products[0].Price,
                        ProductQuantity=3,
                        TotalPrice=3*Products[0].Price
                    }
                }
            });
        }
    }
}
