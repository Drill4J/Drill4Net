﻿using System;
using System.Linq;
using AutoMapper;
using Drill4Net.Demo.OnlineStore.Bll.Contracts.Interfaces;
using Drill4Net.Demo.OnlineStore.Bll.Contracts.Models;
using Drill4Net.Demo.OnlineStore.Dal.Helpers;

namespace Drill4Net.Demo.OnlineStore.Dal.Services
{
    public class CartInMemoryStorage : ICartDataReadService, ICartDataWriteService
    {
        private readonly IMapper _mapper;

        /******************************************************************/

        public CartInMemoryStorage(IMapper mapper)
        {
            _mapper = mapper;
        }

        /******************************************************************/

        public Cart GetCart(Guid id)
        {
            var dalItem = DataContext.Carts.FirstOrDefault(x => x.Id == id);
            return _mapper.Map<Bll.Contracts.Models.Cart>(dalItem);
        }
        public Cart Create(Cart item)
        {
            var dalItem = _mapper.Map<Dal.Models.Cart>(item);
            DataContext.Carts.Add(dalItem);
            return item;
        }
        public void AddToCart(Guid cartId, Guid productId, int amount)
        {
            var cart = DataContext.Carts.FirstOrDefault(x => x.Id == cartId);
            if (cart != null)
            {
                var cartItem = CartDataHelper.GetCartItem(cartId, productId);
                if (cartItem != null)
                {
                    ChangeItemAmount(cartId, productId, amount + cartItem.ProductQuantity);
                }
                else
                {
                    var product = DataContext.Products.FirstOrDefault(x => x.Id == productId);
                    if (product != null)
                    {
                        cart.Products.Add(new Models.CartItem
                        {
                            ProductId = productId,
                            ProductName = product.Name,
                            ProductPrice = product.Price,
                            ProductQuantity = amount
                        });
                    }

                }
            }
        }
        public void ChangeItemAmount(Guid cartId, Guid productId, int amount)
        {
            var cartItem = CartDataHelper.GetCartItem(cartId, productId);
            if (cartItem != null)
            {
                cartItem.ProductQuantity = amount;
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
