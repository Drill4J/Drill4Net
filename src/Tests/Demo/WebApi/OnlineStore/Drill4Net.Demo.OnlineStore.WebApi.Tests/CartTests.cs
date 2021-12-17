using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Hosting;
using Drill4Net.Demo.OnlineStore.WebApi.Host;
using Drill4Net.Demo.OnlineStore.WebApi.Models;

namespace Drill4Net.Demo.OnlineStore.WebApi.Tests
{
    public class CartTests : IDisposable
    {
        private readonly TestServer _server;
        private readonly HttpClient _client;

        public CartTests()
        {
            // Arrange
            _server = new TestServer(new WebHostBuilder()
               .UseStartup<Startup>());
            _client = _server.CreateClient();
        }

        //Create new cart. Add product to cart and check that product is in the cart.
        //Change product quantity, check total.
        //Delete product, check that cart is empty.

        [Theory]
        [InlineData("Cat1", "TestProduct", "10.20", 16, 4)]
        public async Task AddProductToCart(string category, string name, string price, int stock, int newAmount)
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
            var responseNewCart = await _client.PostAsJsonAsync("/api/Carts", new NewCartDto());
            responseNewCart.EnsureSuccessStatusCode();
            var cart = await responseNewCart.Content.ReadAsAsync<CartDto>();
            await _client.PutAsJsonAsync($"/api/Carts/{cart.Id}/items/{product.Id}/add", 1);
            cart = await GetCart(cart.Id);

            // Assert
            Assert.NotNull(cart.Products.FirstOrDefault(i=>i.ProductId==product.Id));

            // Act
            await _client.PutAsJsonAsync($"/api/Carts/{cart.Id}/items/{product.Id}/change", newAmount);
            cart = await GetCart(cart.Id);

            // Assert
            Assert.NotNull(cart.Products.FirstOrDefault(i => i.ProductId == product.Id));
            Assert.Equal(newAmount * convertedPrice, (cart.Products.FirstOrDefault(i => i.ProductId == product.Id).TotalPrice));
            Assert.Equal(newAmount*convertedPrice, cart.Total);

            // Act
            await _client.DeleteAsync($"/api/Carts/{cart.Id}/items/{product.Id}");
            cart = await GetCart(cart.Id);

            // Assert
            Assert.Null(cart.Products.FirstOrDefault(i => i.ProductId == product.Id));
        }

        private async Task<CartDto> GetCart(Guid id)
        {
            var responseGetCart = await _client.GetAsync($"/api/Carts/{id}");
            responseGetCart.EnsureSuccessStatusCode();
            return  await responseGetCart.Content.ReadAsAsync<CartDto>();
        }

        public void Dispose()
        {
            _client.Dispose();
            _server.Dispose();
        }
    }
}
