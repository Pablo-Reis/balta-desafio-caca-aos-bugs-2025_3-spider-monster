using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using BugStore.Data;
using BugStore.Handlers.Orders;
using BugStore.Interfaces.Handlers;
using BugStore.Models;
using BugStore.Requests.Orders;
using BugStore.Requests.Products;
using BugStore.Responses;
using BugStore.Requests.OrderLines;

namespace BugStore.Test.Handlers.Orders
{
    public class OrderHandlerTests
    {
        private static AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private class FaultyDbContext : AppDbContext
        {
            public FaultyDbContext(DbContextOptions options) : base(options) { }

            public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            {
                throw new Exception("Simulated DB error");
            }
        }

        private class FakeProductHandler : IProductHandler
        {
            private readonly List<Product> _products;
            public FakeProductHandler(IEnumerable<Product> products) => _products = products.ToList();

            public Task<Response<Product>> CreateProductAsync(CreateProductRequest request) => throw new NotImplementedException();
            public Task<Response<Product>> UpdateProductAsync(UpdateProductRequest request) => throw new NotImplementedException();
            public Task<Response<Product>> DeleteProductAsync(DeleteProductRequest request) => throw new NotImplementedException();
            public Task<Response<Product>> GetProductByIdAsync(GetProductByIdRequest request) => throw new NotImplementedException();

            public Task<Response<List<Product>>> GetProductsAsync(GetProductsRequest request)
                => Task.FromResult(new Response<List<Product>>(_products));
        }

        [Fact]
        public async Task CreateOrderAsync_CreatesOrder_With_CorrectLineTotals()
        {
            //Arrange
            await using var context = CreateContext();

            //Seed data
            var product1 = new Product { Id = Guid.NewGuid(), Title = "Produto A", Description = "Primeiro produto", Slug = "https://localhost:4000", Price = 10.50m };
            var product2 = new Product { Id = Guid.NewGuid(), Title = "Produto B", Description = "Segundo produto", Slug = "https://localhost:5000", Price = 22.50m };
            var customer = new Customer { Id = Guid.NewGuid(), Name = "Customer de Teste Silva", Email = "customersilva@gmail.com", Phone = "30111033033", BirthDate = DateTime.Today.AddYears(-30) };
            
            var fakeProductHandler = new FakeProductHandler(new[] { product1, product2 });
            var handler = new OrderHandler(context, fakeProductHandler);

            context.Products.AddRange(product1, product2);
            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            var request = new CreateOrderRequest
            {
                CustomerId = customer.Id,
                Lines = new List<CreateOrderLineRequest>
                {
                    new CreateOrderLineRequest { ProductId = product1.Id, Quantity = 2 },
                    new CreateOrderLineRequest { ProductId = product2.Id, Quantity = 3 }
                }
            };

            // Act
            var response = await handler.CreateOrderAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Null(response.Message);
            Assert.NotNull(response.Data);
            Assert.NotEqual(Guid.Empty, response.Data.Id);
            Assert.Equal(2, response.Data.Lines.Count);
            Assert.Equal(customer.Id, response.Data.Customer.Id);
            Assert.True(response.IsSuccess);

            var line1 = response.Data.Lines.First(l => l.ProductId == product1.Id);
            var line2 = response.Data.Lines.First(l => l.ProductId == product2.Id);

            Assert.Equal(product1.Price * 2, line1.Total);
            Assert.Equal(product2.Price * 3, line2.Total);

            // verify persisted in DB
            var persisted = await context.Orders
                .Include(o => o.Lines)
                .FirstOrDefaultAsync(o => o.Id == response.Data.Id);

            Assert.NotNull(persisted);
            Assert.Equal(2, persisted.Lines.Count);
            Assert.Equal(product1.Price * 2, persisted.Lines.First(l => l.ProductId == product1.Id).Total);
        }

        [Fact]
        public async Task CreateOrderAsync_When_SaveChangesThrows_ReturnsError()
        {
            //Arrange
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            List<Product> _products;
            await using (var seed = new AppDbContext(options))
            {
                _products = new List<Product>
                {
                    new Product { Id = Guid.NewGuid(), Title = "P1", Description = "d1", Slug = "s1", Price = 19.9m }
                };
                seed.Products.AddRange(_products);
                await seed.SaveChangesAsync();
            }

            var fakeProductHandler = new FakeProductHandler(_products);

            await using var faulty = new FaultyDbContext(options);
            var handler = new OrderHandler(faulty, fakeProductHandler);

            var request = new CreateOrderRequest
            {
                CustomerId = Guid.NewGuid(),
                Lines = new List<CreateOrderLineRequest>
                {
                    new CreateOrderLineRequest { ProductId = _products[0].Id, Quantity = 1 }
                }
            };

            // Act
            var response = await handler.CreateOrderAsync(request);


            // Assert
            Assert.NotNull(response);
            Assert.Null(response.Data);
            Assert.False(string.IsNullOrWhiteSpace(response.Message));
            Assert.False(response.IsSuccess);
        }

        [Fact]
        public async Task GetOrderByIdAsync_ReturnsOrder_With_Customer_And_ProductNavigation()
        {
            await using var context = CreateContext();

            var product = new Product { Id = Guid.NewGuid(), Title = "P1", Description = "D1", Slug = "S1", Price = 9.9m };
            var customer = new Customer { Id = Guid.NewGuid(), Name = "C1", Email = "customer@gmail", Phone = "123456789", BirthDate = DateTime.Today.AddYears(-35) };

            // create order and orderline and persist so Include will resolve navigation props
            var order = new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = customer.Id,
                CreatedAt = DateTime.UtcNow,
                Lines = new List<OrderLine>
                {
                    new OrderLine { Id = Guid.NewGuid(), ProductId = product.Id, Quantity = 4, Total = product.Price * 4 }
                }
            };

            context.Customers.Add(customer);
            context.Products.Add(product);
            context.Orders.Add(order);
            await context.SaveChangesAsync();

            var fakeProductHandler = new FakeProductHandler(new[] { product });
            var handler = new OrderHandler(context, fakeProductHandler);

            var response = await handler.GetOrderByIdAsync(new GetOrderByIdRequest { Id = order.Id });

            Assert.NotNull(response);
            Assert.Null(response.Message);
            Assert.NotNull(response.Data);
            Assert.Equal(order.Id, response.Data.Id);
            Assert.NotNull(response.Data.Customer);
            Assert.Equal(customer.Id, response.Data.Customer.Id);
            Assert.NotNull(response.Data.Lines);
            Assert.Single(response.Data.Lines);
            Assert.NotNull(response.Data.Lines[0].Product);
            Assert.Equal(product.Id, response.Data.Lines[0].Product.Id);
            Assert.True(response.IsSuccess);
        }

        [Fact]
        public async Task GetOrderByIdAsync_ReturnsNotFound_When_Missing()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var context = CreateContext();

            var fakeProductHandler = new FakeProductHandler(Array.Empty<Product>());
            var handler = new OrderHandler(context, fakeProductHandler);

            var response = await handler.GetOrderByIdAsync(new GetOrderByIdRequest { Id = Guid.NewGuid() });

            Assert.NotNull(response);
            Assert.Null(response.Data);
            Assert.False(response.IsSuccess);
        }

        [Fact]
        public async Task GetOrderByIdAsync_When_DbThrows_ReturnsErrorMessage()
        {
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            await using var faulty = new FaultyDbContext(options);
            var fakeProductHandler = new FakeProductHandler(Array.Empty<Product>());
            var handler = new OrderHandler(faulty, fakeProductHandler);

            var response = await handler.GetOrderByIdAsync(new GetOrderByIdRequest { Id = Guid.NewGuid() });

            Assert.NotNull(response);
            Assert.Null(response.Data);
            Assert.False(response.IsSuccess);
        }
    }
}
