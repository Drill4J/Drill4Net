using AutoMapper;
using Drill4Net.Demo.OnlineStore.Bll.Contracts.Interfaces;
using Drill4Net.Demo.OnlineStore.Bll.Contracts.Models;
using Drill4Net.Demo.OnlineStore.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Drill4Net.Demo.OnlineStore.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class CartsController : ControllerBase
    {
        private readonly IMapper _mapper;

        private readonly ICartBusinessService _cartBllService;
        private readonly ICartDataReadService _cartDalService;

        public CartsController(ICartBusinessService cartBllService, ICartDataReadService cartDalService, IMapper mapper)
        {
            _cartBllService = cartBllService;
            _cartDalService = cartDalService;
            _mapper = mapper;
        }
        [HttpGet]
        public CartDto GetCart(Guid cartId)
        {
            var cart = _cartDalService.GetCart(cartId);
            return _mapper.Map<CartDto>(cart);
        }

        [HttpPost]
        public ActionResult<ProductDto> CreateCart(NewCartDto cartDto)
        {
            var newCart = _cartBllService.CreateCart(_mapper.Map<Cart>(cartDto));
            return Created(new Uri($"/{newCart.Id}", UriKind.Relative), _mapper.Map<CartDto>(newCart));
        }

        [HttpPut("{id}")]
        public ActionResult AddToCart(Guid cartId, ProductDto product)
        {
            var cartItem = _mapper.Map<CartItem>(product);
            _cartBllService.AddToCart(cartId, cartItem);
            return NoContent();
        }

        [HttpPut("{id}")]
        public ActionResult ChangeCartItemAmount(Guid cartId, Guid productId, int amount)
        {
            _cartBllService.ChangeCartItemAmount(cartId, productId, amount);
            return NoContent();
        }

        [HttpPut("{id}")]
        public ActionResult RemoveFromCart(Guid cartId, Guid productId)
        {
            _cartBllService.RemoveFromCart(cartId, productId);
            return NoContent();
        }
    }
}
