using System;
using System.Linq;

namespace Drill4Net.Demo.OnlineStore.Dal.Helpers
{
    internal static class CartDataHelper
    {
        internal static Models.Cart GetCart(Guid id)
        {
            return DataContext.Carts.FirstOrDefault(x => x.Id == id);
        }
        internal static Models.CartItem GetCartItem(Guid cartId, Guid productId)
        {
            if(GetCart(cartId)==null)
            {
                return null;
            }
            return DataContext.Carts.First(x => x.Id == cartId).Products.FirstOrDefault(i=>i.ProductId==productId);
        }
    }
}
