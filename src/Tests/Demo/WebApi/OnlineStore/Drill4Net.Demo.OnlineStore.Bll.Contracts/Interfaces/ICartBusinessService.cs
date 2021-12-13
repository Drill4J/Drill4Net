using System;
using Drill4Net.Demo.OnlineStore.Bll.Contracts.Models;

namespace Drill4Net.Demo.OnlineStore.Bll.Contracts.Interfaces
{
    public interface ICartBusinessService
    {
        public Cart CreateCart(Cart cart);
        public void AddToCart(Guid cartId, Guid productId, int amount);
        public void RemoveFromCart(Guid cartId, Guid productId);
        public void ChangeCartItemAmount(Guid cartId, Guid productId, int amount);
    }
}
