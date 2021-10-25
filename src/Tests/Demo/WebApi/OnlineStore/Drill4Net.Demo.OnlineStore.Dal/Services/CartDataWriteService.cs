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

        public void AddToCart(Guid cartId, Guid productId, int amount)
        {
            var cart = CartDataHelper.GetCart(cartId);
            if (cart != null)
            {
                var cartItem = CartDataHelper.GetCartItem(cartId, productId);
                if (cartItem != null)
                {
                    ChangeItemAmount(cartId, productId, amount+cartItem.ProductQuantity);
                }
                else
                {
                    var product = ProductDataHelper.GetProduct(productId);
                    if (product!=null)
                    {
                        cart.Products.Add(new Models.CartItem 
                        { 
                            ProductId=productId,
                            ProductName=product.Name,
                            ProductPrice=product.Price,
                            ProductQuantity=amount
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
