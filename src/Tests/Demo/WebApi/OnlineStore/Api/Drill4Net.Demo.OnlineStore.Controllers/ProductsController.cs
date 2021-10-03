using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using AutoMapper;
using Drill4Net.Demo.OnlineStore.Dal.Interfaces;
using Drill4Net.Demo.OnlineStore.WebApi.Models;

namespace Drill4Net.Demo.OnlineStore.WebApi.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IMapper _mapper;

        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // GET: api/products/sorted
        //[HttpGet]
        //[Route("Sorted")]
        //public IEnumerable<ProductDto> Get(int page, int pageItemsNumber, string sortField)
        //{
        //    var config = new MapperConfiguration(cfg => cfg.CreateMap<ProductDto, Product>());
        //    var mapper = new Mapper(config);
        //    var products = _mapper.Map<List<ProductDto>>(DataContext.Products.Skip((page - 1) * pageItemsNumber).Take(pageItemsNumber));
        //    switch (sortField)
        //    {
        //        case "name":
        //            products = products.OrderBy(p => p.Name).ToList();
        //            break;
        //        case "price":
        //            products = products.OrderBy(p => p.Price).ToList();
        //            break;
        //        case "stock":
        //            products = products.OrderBy(p => p.Stock).ToList();
        //            break;
        //        default:
        //            goto case "name";
        //    }
        //    return products;
        //}

        // GET: api/products
        [HttpGet]
        public IEnumerable<ProductDto> Get()
        {
            var products = _mapper.Map<List<ProductDto>>(_productService.GetAll());
            return products;
        }

    }
}

