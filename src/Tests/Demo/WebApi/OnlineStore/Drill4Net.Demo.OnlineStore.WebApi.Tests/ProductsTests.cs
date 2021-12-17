using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Hosting;
using Drill4Net.Demo.OnlineStore.WebApi.Host;
using Drill4Net.Demo.OnlineStore.WebApi.Models;

namespace Drill4Net.Demo.OnlineStore.WebApi.Tests
{
    public class ProductsTests:IDisposable
    {
        private readonly TestServer _server;
        private readonly HttpClient _client;

        public ProductsTests()
        {
            // Arrange
            _server = new TestServer(new WebHostBuilder()
               .UseStartup<Startup>());
            _client = _server.CreateClient();
        }

       //Add new product
       //Get information about this product
       //Check that information is correct.

        [Theory]
        [InlineData("Cat1", "TestProduct", "10.20", 16)]
        public async Task CreateNewProduct( string category, string name, string price, int stock)
        {
            // Arrange
            var convertedPrice = Convert.ToDecimal(price);
            var newProduct = new NewProductDto()
            {
                Category = category,
                Name = name,
                Price = convertedPrice,
                Stock = stock
            };

            // Act
            var responseNewProduct = await _client.PostAsJsonAsync("/api/Products", newProduct);
            responseNewProduct.EnsureSuccessStatusCode();
            var product = await responseNewProduct.Content.ReadAsAsync<ProductDto>();
            var responseGetProduct = await _client.GetAsync($"/api/Products/{product.Id}");
            responseGetProduct.EnsureSuccessStatusCode();
            var createdProduct=await responseGetProduct.Content.ReadAsAsync<ProductDto>();

            // Assert
            Assert.NotNull(createdProduct);
            Assert.Equal(category, createdProduct.Category);
            Assert.Equal(name, createdProduct.Name);
            Assert.Equal(convertedPrice, createdProduct.Price);
            Assert.Equal(stock, createdProduct.Stock);
        }

        public void Dispose()
        {
            _client.Dispose();
            _server.Dispose();
        }
    }
}
