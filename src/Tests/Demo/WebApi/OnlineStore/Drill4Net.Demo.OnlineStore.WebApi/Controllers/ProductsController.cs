using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Drill4Net.Demo.OnlineStore.WebApi.Models;
using Drill4Net.Demo.OnlineStore.Dal;
using AutoMapper;

namespace Drill4Net.Demo.OnlineStore.WebApi.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly Mapper _mapper= new Mapper(new MapperConfiguration(cfg => cfg.CreateMap<Product, ProductDto>()));
        //ProductsController()
        //{
        //    var config = new MapperConfiguration(cfg => cfg.CreateMap<ProductDto, Product>());
        //    _mapper = new Mapper(config);
        //}
        // GET: api/products
        [HttpGet]
        [Route("Sorted")]
        public List<ProductDto> Get (int  page, int pageItemsNumber, string sortField)
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<ProductDto, Product>());
            var mapper = new Mapper(config);
            var products = _mapper.Map<List<ProductDto>>(DataContext.Products.Skip((page-1)*pageItemsNumber).Take(pageItemsNumber));
            switch (sortField)
            {
                case "name":
                    products = products.OrderBy(p => p.Name).ToList();
                    break;
                case "price":
                    products = products.OrderBy(p => p.Price).ToList();
                    break;
                case "stock":
                    products = products.OrderBy(p => p.Stock).ToList();
                    break;
                default:
                    goto case "name";
            }
            return products;
        }
        [HttpGet]
        public List<ProductDto> Get()
        {
            var products = _mapper.Map<List<ProductDto>>(DataContext.Products);
            return products;
        }
    }
}
