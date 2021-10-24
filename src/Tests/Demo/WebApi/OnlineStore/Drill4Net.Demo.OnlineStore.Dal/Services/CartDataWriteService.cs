using AutoMapper;
using Drill4Net.Demo.OnlineStore.Bll.Contracts.Interfaces;
using Drill4Net.Demo.OnlineStore.Bll.Contracts.Models;
using Drill4Net.Demo.OnlineStore.Dal.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drill4Net.Demo.OnlineStore.Dal.Services
{
    public class CartDataWriteService : ICartDataWriteService
    {
        private readonly IMapper _mapper;
        public CartDataWriteService(IMapper mapper)
        {
            _mapper = mapper;
        }
        public Cart Create(Cart item)
        {
            var dalItem= _mapper.Map<Dal.Models.Cart>(item);
            DataContext.Carts.Add(dalItem);
            return item;
        }

        public void AddToCart(Guid cartId, CartItem item)
        {
            var cart = CartDataHelper.GetCart(cartId);
            if (cart != null)
            {
                var cartItem = CartDataHelper.GetCartItem(cartId, item.ProductId);
                if (cartItem != null)
                {
                    ChangeItemAmount(cartId, item.ProductId, item.ProductQuantity+cartItem.ProductQuantity);
                }
                else
                {
                    cart.Products.Add(_mapper.Map<Dal.Models.CartItem>(item));
                }
            }
        }
        public void ChangeItemAmount(Guid cartId, Guid productId, int amount)
        {
                var cartItem = CartDataHelper.GetCartItem(cartId, productId);
                if (cartItem != null)
                {
                    cartItem.ProductQuantity=amount;
                }
        }

        public void RemoveFromCart(Guid cartId, Guid productId)
        {
            var cartItem = CartDataHelper.GetCartItem(cartId, productId);
            if (cartItem != null)
            {
                var cart = CartDataHelper.GetCart(cartId);
                cart.Products.Remove(cartItem);
            }
        }
    }
}
