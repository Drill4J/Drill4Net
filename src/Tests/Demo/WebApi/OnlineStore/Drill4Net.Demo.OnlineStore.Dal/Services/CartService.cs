using Drill4Net.Demo.OnlineStore.Dal.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drill4Net.Demo.OnlineStore.Dal.Services
{
    public class CartService : IServiceBase<Cart>
    {
        public void Create(Cart item)
        {
            item.Id = Guid.NewGuid();
            DataContext.Carts.Add(item);
        }

        public Cart Get(Guid id)
        {
           return DataContext.Carts.FirstOrDefault(x => x.Id == id);
        }

        public void Update(Cart item)
        {
            var cartItem = Get(item.Id);
            if (cartItem!=null)
            {
                cartItem.Products = item.Products;
                cartItem.Total = item.Total;
            }
        }
    }
}
