using System;
using Drill4Net.Demo.OnlineStore.Bll.Contracts.Interfaces;
using Drill4Net.Demo.OnlineStore.Bll.Contracts.Models;

namespace Drill4Net.Demo.OnlineStore.Bll.Services
{
    public class CartService : ICartBusinessService
    {
        private readonly ICartDataWriteService _cartDataServiceWrite;

        /******************************************************************/

        public CartService(ICartDataWriteService cartDataServiceWrite)
        {
            _cartDataServiceWrite = cartDataServiceWrite;
        }

        /******************************************************************/

        public void AddToCart(Guid cartId, Guid productId, int amount)
        {
            _cartDataServiceWrite.AddToCart(cartId, productId, amount);
        }

        public void ChangeCartItemAmount(Guid cartId, Guid productId, int amount)
        {
            _cartDataServiceWrite.ChangeItemAmount(cartId, productId, amount);
        }

        public Cart CreateCart(Cart cart)
        {
            return _cartDataServiceWrite.Create(cart);
        }

        public void RemoveFromCart(Guid cartId, Guid productId)
        {
            _cartDataServiceWrite.RemoveFromCart(cartId, productId);
        }
    }
}
