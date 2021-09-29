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
using Drill4Net.Demo.OnlineStore.Dal.Interfaces;

namespace Drill4Net.Demo.OnlineStore.WebApi.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly Mapper _mapper = new Mapper(new MapperConfiguration(cfg => cfg.CreateMap<Product, ProductDto>()));


        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // GET: api/products/sorted
        [HttpGet]
        [Route("Sorted")]
        public IEnumerable<ProductDto> Get(int page, int pageItemsNumber, string sortField)
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<ProductDto, Product>());
            var mapper = new Mapper(config);
            var products = _mapper.Map<List<ProductDto>>(DataContext.Products.Skip((page - 1) * pageItemsNumber).Take(pageItemsNumber));
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

        // GET: api/products
        [HttpGet]
        public IEnumerable<ProductDto> Get()
        {
            var products = _mapper.Map<List<ProductDto>>(_productService.GetAll());
            return products;
        }
        // POST: api/products/
        //[HttpPost]
        //public HttpResponseMessage Post(ProductDto product)
        //{
        //    try
        //    {
        //        libraryDbContext.Newspapers.Add(new Newspaper(newspaper.PublicationPlace, newspaper.PublicationTitle, newspaper.Number,
        //            newspaper.PublicationYear, newspaper.Date, newspaper.NumberOfCopies, newspaper.ISSN, newspaper.Title, newspaper.Notes,
        //            newspaper.NumberOfPages, newspaper.Cost));
        //        libraryDbContext.SaveChanges();
        //        var response = Request.CreateResponse<NewspaperDto>(HttpStatusCode.Created, product);
        //        string uri = Url.RouteUrl(null, new { id = newspaper.ISSN });
        //        response.Headers.Location = new Uri(Request, Request., uri);
        //        return response;
        //    }
        //    catch
        //    {
        //        throw new HttpResponseException(HttpStatusCode.BadRequest);
        //    }
        //    var products = _mapper.Map<List<ProductDto>>(DataContext.Products);
        //    return products;
        //}
        //public HttpResponseMessage Put(string id, NewspaperDto newspaperData)
        //{
        //    try
        //    {
        //        var newspaper = libraryDbContext.Newspapers.FirstOrDefault(n => n.ISSN == id);
        //        newspaper.Title = newspaperData.Title;
        //        newspaper.Cost = newspaperData.Cost;
        //        newspaper.Date = newspaperData.Date;
        //        newspaper.Notes = newspaperData.Notes;
        //        newspaper.Number = newspaperData.Number;
        //        newspaper.NumberOfCopies = newspaperData.NumberOfCopies;
        //        newspaper.NumberOfPages = newspaperData.NumberOfPages;
        //        newspaper.PublicationPlace = newspaperData.PublicationPlace;
        //        newspaper.PublicationTitle = newspaperData.PublicationTitle;
        //        libraryDbContext.SaveChanges();
        //        return new HttpResponseMessage(HttpStatusCode.NoContent);
        //    }
        //    catch
        //    {
        //        throw new HttpResponseException(HttpStatusCode.BadRequest);

        //    }
        //}
    }
}

