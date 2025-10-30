using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using BugStore.Data;
using BugStore.Handlers.Products;
using BugStore.Models;
using BugStore.Requests.Products;
using BugStore.Responses;

namespace BugStore.Test.Handlers.Products
{
    public class ProductHandlerTests
    {
        private static AppDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new AppDbContext(options);
        }

        private class FaultyDbContext : AppDbContext
        {
            public FaultyDbContext(DbContextOptions options) : base(options) { }

            public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
                => throw new Exception("Simulated DB failure");
        }

        [Fact]
        public async Task CreateProductAsync_Should_CreateProduct()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var context = CreateContext(dbName);
            var handler = new ProductHandler(context);

            var request = new CreateProductRequest
            {
                Title = "P1",
                Description = "Desc",
                Price = 12.34m,
                Slug = "p1"
            };

            var response = await handler.CreateProductAsync(request);

            Assert.NotNull(response);
            Assert.Null(response.Message);
            Assert.NotNull(response.Data);
            Assert.Equal(request.Title, response.Data.Title);
            Assert.Equal(request.Price, response.Data.Price);

            var persisted = await context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Title == request.Title);
            Assert.NotNull(persisted);
            Assert.Equal(request.Slug, persisted.Slug);
        }

        [Fact]
        public async Task CreateProductAsync_When_SaveChangesFails_ReturnsErrorResponse()
        {
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            await using var faulty = new FaultyDbContext(options);
            var handler = new ProductHandler(faulty);

            var request = new CreateProductRequest
            {
                Title = "Pfail",
                Description = "D",
                Price = 1.0m,
                Slug = "pfail"
            };

            var response = await handler.CreateProductAsync(request);

            Assert.NotNull(response);
            Assert.Null(response.Data);
            Assert.False(string.IsNullOrWhiteSpace(response.Message));
            Assert.Contains("Ocorreu um erro ao criar produto", response.Message);
        }

        [Fact]
        public async Task DeleteProductAsync_Should_DeleteExistingProduct()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var context = CreateContext(dbName);

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Title = "ToDelete",
                Description = "d",
                Price = 2.0m,
                Slug = "td"
            };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var handler = new ProductHandler(context);
            var response = await handler.DeleteProductAsync(new DeleteProductRequest { Id = product.Id });

            Assert.NotNull(response);
            Assert.Null(response.Message);
            Assert.NotNull(response.Data);
            Assert.Equal(product.Id, response.Data.Id);

            var exists = await context.Products.AnyAsync(p => p.Id == product.Id);
            Assert.False(exists);
        }

        [Fact]
        public async Task DeleteProductAsync_Should_ReturnNotFound_When_Missing()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var context = CreateContext(dbName);
            var handler = new ProductHandler(context);

            var response = await handler.DeleteProductAsync(new DeleteProductRequest { Id = Guid.NewGuid() });

            Assert.NotNull(response);
            Assert.Null(response.Data);
            Assert.Equal("Product not found.", response.Message);
        }

        [Fact]
        public async Task DeleteProductAsync_When_SaveChangesFails_ReturnsErrorResponse()
        {
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            // seed normal
            await using (var seed = new AppDbContext(options))
            {
                seed.Products.Add(new Product { Id = Guid.NewGuid(), Title = "S", Description = "s", Price = 1m, Slug = "s" });
                await seed.SaveChangesAsync();
            }

            await using var faulty = new FaultyDbContext(options);
            var toDelete = await faulty.Products.AsNoTracking().FirstOrDefaultAsync();
            var handler = new ProductHandler(faulty);

            var response = await handler.DeleteProductAsync(new DeleteProductRequest { Id = toDelete.Id });

            Assert.NotNull(response);
            Assert.Null(response.Data);
            Assert.False(string.IsNullOrWhiteSpace(response.Message));
            Assert.Contains("Ocorreu um erro ao deletar produto", response.Message);
        }

        [Fact]
        public async Task GetProductsAsync_Should_ReturnAllProducts()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var context = CreateContext(dbName);

            context.Products.AddRange(new[]
            {
                new Product { Id = Guid.NewGuid(), Title = "A", Description = "d", Price = 1m, Slug = "a" },
                new Product { Id = Guid.NewGuid(), Title = "B", Description = "d2", Price = 2m, Slug = "b" }
            });
            await context.SaveChangesAsync();

            var handler = new ProductHandler(context);
            var response = await handler.GetProductsAsync(new GetProductsRequest());

            Assert.NotNull(response);
            Assert.Null(response.Message);
            Assert.NotNull(response.Data);
            Assert.Equal(2, response.Data.Count);
        }

        [Fact]
        public async Task GetProductByIdAsync_Should_ReturnProduct_WhenExists()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var context = CreateContext(dbName);

            var product = new Product { Id = Guid.NewGuid(), Title = "C", Description = "d", Price = 3m, Slug = "c" };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var handler = new ProductHandler(context);
            var response = await handler.GetProductByIdAsync(new GetProductByIdRequest { Id = product.Id });

            Assert.NotNull(response);
            Assert.Null(response.Message);
            Assert.NotNull(response.Data);
            Assert.Equal(product.Id, response.Data.Id);
        }

        [Fact]
        public async Task GetProductByIdAsync_Should_ReturnNull_WhenMissing()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var context = CreateContext(dbName);

            var handler = new ProductHandler(context);
            var response = await handler.GetProductByIdAsync(new GetProductByIdRequest { Id = Guid.NewGuid() });

            Assert.NotNull(response);
            Assert.Null(response.Data);
        }

        [Fact]
        public async Task UpdateProductAsync_Should_UpdateExistingProduct()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var context = CreateContext(dbName);

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Title = "Before",
                Description = "D",
                Price = 9.99m,
                Slug = "before"
            };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var handler = new ProductHandler(context);
            var request = new UpdateProductRequest
            {
                Id = product.Id,
                Title = "After",
                Description = "D2",
                Price = 19.99m,
                Slug = "after"
            };

            var response = await handler.UpdateProductAsync(request);

            Assert.NotNull(response);
            Assert.Null(response.Message);
            Assert.NotNull(response.Data);
            Assert.Equal(request.Title, response.Data.Title);
            Assert.Equal(request.Price, response.Data.Price);

            var persisted = await context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == product.Id);
            Assert.Equal(request.Title, persisted.Title);
            Assert.Equal(request.Price, persisted.Price);
        }

        [Fact]
        public async Task UpdateProductAsync_Should_ReturnNotFound_When_Missing()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var context = CreateContext(dbName);

            var handler = new ProductHandler(context);
            var response = await handler.UpdateProductAsync(new UpdateProductRequest { Id = Guid.NewGuid(), Title = "X", Description = "x", Price = 1m, Slug = "x" });

            Assert.NotNull(response);
            Assert.Null(response.Data);
            Assert.Equal("Product not found.", response.Message);
        }

        [Fact]
        public async Task UpdateProductAsync_When_SaveChangesFails_ReturnsErrorResponse()
        {
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            // seed normally
            await using (var seed = new AppDbContext(options))
            {
                seed.Products.Add(new Product { Id = Guid.NewGuid(), Title = "S", Description = "s", Price = 1m, Slug = "s" });
                await seed.SaveChangesAsync();
            }

            await using var faulty = new FaultyDbContext(options);
            var existing = await faulty.Products.AsNoTracking().FirstOrDefaultAsync();
            var handler = new ProductHandler(faulty);

            var request = new UpdateProductRequest
            {
                Id = existing.Id,
                Title = "New",
                Description = "New",
                Price = 2m,
                Slug = "new"
            };

            var response = await handler.UpdateProductAsync(request);

            Assert.NotNull(response);
            Assert.Null(response.Data);
            Assert.False(string.IsNullOrWhiteSpace(response.Message));
            Assert.Contains("Ocorreu um erro ao deletar produto", response.Message);
        }
    }
}
