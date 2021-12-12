using System;
using System.Collections.Generic;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Drill4Net.Demo.OnlineStore.WebApi.Models;
using Drill4Net.Demo.OnlineStore.Bll.Contracts.Interfaces;

using Drill4Net.Demo.OnlineStore.Bll.Contracts.Models;

namespace Drill4Net.Demo.OnlineStore.WebApi.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IProductBusinessService _productBllService;
        private readonly IProductDataReadService _productDalService;

        /******************************************************************/

        public ProductsController(IProductBusinessService productBllService, IProductDataReadService productDalService, IMapper mapper)
        {
            _productBllService = productBllService;
            _productDalService = productDalService;
            _mapper = mapper;
        }
       
        /******************************************************************/

        [HttpGet("sorted")]
        public IEnumerable<ProductInfoDto> GetSortedProductsByPage(int page, int pageItemsNumber, string sortField)
        {
            var products = _productDalService.GetSortedProductsByPage(page, pageItemsNumber, sortField);
            return _mapper.Map <IEnumerable<ProductInfoDto>>(products);
        }

        [HttpGet("filtered")]
        public IEnumerable<ProductInfoDto> GetFilteredProducts(string category, string namePart)
        {
            var products = _productDalService.GetFilteredProducts(category, namePart);
            return _mapper.Map<IEnumerable<ProductInfoDto>>(products);
        }

        [HttpPost]
        public ActionResult<ProductDto> CreateProduct(NewProductDto productDto)
        {
            var newProduct = _productBllService.AddProduct(_mapper.Map<Product>(productDto));
            return Created(new Uri($"/{newProduct.Id}", UriKind.Relative), _mapper.Map<ProductDto>(newProduct));
        }

        [HttpDelete("{id}")]
        public ActionResult DeleteProduct(Guid id)
        {
            _productBllService.DeleteProduct(id);
            return NoContent();
        }

        [HttpPut("{id}")]
        public ActionResult UpdateProduct(Guid id, ProductDto productDto)
        {
            if (id != productDto.Id)
            {
                return BadRequest();
            }
            _productBllService.UpdateProduct(_mapper.Map<Product>(productDto));
            return NoContent();
        }
    }
}

