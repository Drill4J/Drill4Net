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
    [Route("api/[controller]")]
    public class CartsController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ICartBusinessService _cartBllService;
        private readonly ICartDataReadService _cartDalService;

        /******************************************************************/

        public CartsController(ICartBusinessService cartBllService, ICartDataReadService cartDalService, IMapper mapper)
        {
            _cartBllService = cartBllService;
            _cartDalService = cartDalService;
            _mapper = mapper;
        }

        /******************************************************************/

        [HttpGet("{cartId}")]
        public CartDto GetCart(Guid cartId)
        {
            var cart = _cartDalService.GetCart(cartId);
            return _mapper.Map<CartDto>(cart);
        }

        [HttpPost]
        public ActionResult<CartDto> CreateCart(NewCartDto cartDto)
        {
            var newCart = _cartBllService.CreateCart(_mapper.Map<Cart>(cartDto));
            return Created(new Uri($"/{newCart.Id}", UriKind.Relative), _mapper.Map<CartDto>(newCart));
        }

        [HttpPut("{cartId}/items/{itemId}/add")]
        public ActionResult AddToCart(Guid cartId, Guid itemId, [FromBody] int amount)
        {
            _cartBllService.AddToCart(cartId, itemId, amount);
            return NoContent();
        }

        [HttpPut("{cartId}/items/{itemId}/change")]
        public ActionResult ChangeCartItemAmount(Guid cartId, Guid itemId, [FromBody] int amount)

        {
            _cartBllService.ChangeCartItemAmount(cartId, itemId, amount);
            return NoContent();
        }

        [HttpDelete("{cartId}/items/{itemId}")]
        public ActionResult RemoveFromCart(Guid cartId, Guid itemId)
        {
            _cartBllService.RemoveFromCart(cartId, itemId);
            return NoContent();
        }
    }
}
