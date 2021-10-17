using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using AutoMapper;
using Drill4Net.Demo.OnlineStore.WebApi.Models;
using Drill4Net.Demo.OnlineStore.Bll.Interfaces;
using System;
using Drill4Net.Demo.OnlineStore.Bll.Models;

namespace Drill4Net.Demo.OnlineStore.WebApi.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IMapper _mapper;

        private readonly IProductBusinessService _productBllService;
        private readonly IProductDataReadService _productDalService;

        public ProductsController(IProductBusinessService productBllService, IProductDataReadService productDalService, IMapper mapper)
        {
            _productBllService = productBllService;
            _productDalService = productDalService;
            _mapper = mapper;
        }

       [HttpGet]
        public IEnumerable<ProductDto> GetSortedProductsByPage(int page, int pageItemsNumber, string sortField)
        {
            var products = _productDalService.GetSortedProductsByPage(page, pageItemsNumber, sortField);
            return _mapper.Map <IEnumerable<ProductDto>>(products);
        }

        [HttpPost]
        public ActionResult<ProductDto> CreateProduct(NewProductDto productDto)
        {

            var newProduct = _productBllService.AddProduct(_mapper.Map<Product>(productDto));
            return Created(new Uri($"/{newProduct.Id}", UriKind.Relative), _mapper.Map<ProductDto>(newProduct));
        }
        [HttpDelete("{id}")]
        public ActionResult DeleteProduct(Guid productDtoGuid)
        {
            _productBllService.DeleteProduct(productDtoGuid);
            return NoContent();
        }
        [HttpPut("{id}")]
        public ActionResult UpdateProduct(Guid productDtoGuid, ProductDto productDto)
        {
            if (productDtoGuid != productDto.Id)
            {
                return BadRequest();
            }
            _productBllService.UpdateProduct(_mapper.Map<Product>(productDto));
            return NoContent();
        }
    }
}

