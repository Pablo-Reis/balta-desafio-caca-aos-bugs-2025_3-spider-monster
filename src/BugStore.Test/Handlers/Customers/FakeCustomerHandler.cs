using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using BugStore.Data;
using BugStore.Handlers.Customers;
using BugStore.Models;
using BugStore.Requests.Customers;
using BugStore.Responses;

namespace BugStore.Test.Handlers.Customers
{
    public class CustomerHandlerTests
    {
        private Customer Customer { get; set; } = new Customer { Id = Guid.NewGuid(), Name = "New teste", Email = "teste@gmail.com", Phone = "123456789", BirthDate = DateTime.Today };
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
        public async Task Should_Pass_When_Persist_Valid_Data()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var context = CreateContext(dbName);
            var handler = new CustomerHandler(context);

            var request = new CreateCustomerRequest
            {
                Name = "Teste Novo",
                Email = "novoteste@gmail.com",
                Phone = "123456789",
                BirthDate = new DateTime(1990, 1, 1)
            };

            var response = await handler.CreateCustomerAsync(request);

            Assert.NotNull(response);
            Assert.NotNull(response.Data);
            Assert.Null(response.Message);
            Assert.NotNull(response.Data);
            Assert.NotEqual(new Guid(),response.Data.Id);
            Assert.Equal(request.Name, response.Data.Name);
            Assert.Equal(request.Email, response.Data.Email);
            Assert.Equal(request.Phone, response.Data.Phone);
            Assert.Equal(request.BirthDate, response.Data.BirthDate);

            // verify persisted
            var persisted = await context.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == response.Data.Id);
            
            Assert.NotNull(persisted);
            Assert.Equal(request.Name, persisted.Name);
            Assert.Equal(request.Email, persisted.Email);
            Assert.Equal(request.Phone, persisted.Phone);
            Assert.Equal(request.BirthDate, persisted.BirthDate);
        }

        [Fact]
        public async Task Should_Pass_When_SaveChangesFails_ReturnsErrorResponse()
        {
            var dbName = Guid.NewGuid().ToString();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            await using var context = new FaultyDbContext(options);

            var handler = new CustomerHandler(context);
            var request = new CreateCustomerRequest
                {
                    Name = "Teste Novo",
                    Email = "novoteste@gmail.com",
                    Phone = "123456789",
                    BirthDate = new DateTime(1990, 1, 1)
                };

            var response = await handler.CreateCustomerAsync(request);

            Assert.NotNull(response);
            Assert.Null(response.Data);
            Assert.False(string.IsNullOrWhiteSpace(response.Message));
            Assert.False(response.IsSuccess);
        }

        [Fact]
        public async Task DeleteCustomerAsync_Should_DeleteExistingCustomer()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var context = CreateContext(dbName);

            var customer = Customer;
            context.Customers.Add(customer);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var handler = new CustomerHandler(context);
            var response = await handler.DeleteCustomerAsync(new DeleteCustomerRequest { Id = customer.Id });

            Assert.NotNull(response);
            Assert.Null(response.Message);
            Assert.NotNull(response.Data);
            Assert.Equal(customer.Id, response.Data.Id);

            var exists = await context.Customers.AnyAsync(c => c.Id == customer.Id);
            Assert.False(exists);
        }

        [Fact]
        public async Task DeleteCustomerAsync_Should_ReturnNotFound_When_Missing()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var context = CreateContext(dbName);
            var handler = new CustomerHandler(context);

            var response = await handler.DeleteCustomerAsync(new DeleteCustomerRequest { Id = Guid.NewGuid() });

            Assert.NotNull(response);
            Assert.Null(response.Data);
            Assert.False(string.IsNullOrEmpty(response.Message));
            Assert.False(response.IsSuccess);
        }

        [Fact]
        public async Task DeleteCustomerAsync_When_SaveChangesFails_ReturnsErrorResponse()
        {
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            // seed using normal context
            await using (var context = new AppDbContext(options))
            {
                context.Customers.Add(Customer);
                await context.SaveChangesAsync();
            }

            await using var faulty = new FaultyDbContext(options);
            var toDelete = await faulty.Customers.AsNoTracking().FirstOrDefaultAsync();
            var handler = new CustomerHandler(faulty);

            var response = await handler.DeleteCustomerAsync(new DeleteCustomerRequest { Id = toDelete.Id });

            Assert.NotNull(response);
            Assert.Null(response.Data);
            Assert.False(string.IsNullOrEmpty(response.Message));
            Assert.False(response.IsSuccess);
        }

        [Fact]
        public async Task GetCustomerAsync_Should_ReturnAllCustomers()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var context = CreateContext(dbName);

            context.Customers.AddRange(new[]
            {
                new Customer { Id = Guid.NewGuid(), Name = "A", Email = "a@a", Phone = "1", BirthDate = DateTime.Today },
                new Customer { Id = Guid.NewGuid(), Name = "B", Email = "b@b", Phone = "2", BirthDate = DateTime.Today }
            });
            await context.SaveChangesAsync();

            var handler = new CustomerHandler(context);
            var response = await handler.GetCustomerAsync(new GetCustomersRequest());

            Assert.NotNull(response);
            Assert.Null(response.Message);
            Assert.NotNull(response.Data);
            Assert.Equal(2, response.Data.Count);
        }

        [Fact]
        public async Task GetCustomerByIdAsync_Should_ReturnCustomer_WhenExists()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var context = CreateContext(dbName);

            var customer = new Customer { Id = Guid.NewGuid(), Name = "C", Email = "c@c", Phone = "3", BirthDate = DateTime.Today };
            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            var handler = new CustomerHandler(context);
            var response = await handler.GetCustomerByIdAsync(new GetCustomerByIdRequest { Id = customer.Id });

            Assert.NotNull(response);
            Assert.Null(response.Message);
            Assert.NotNull(response.Data);
            Assert.Equal(customer.Id, response.Data.Id);
        }

        [Fact]
        public async Task GetCustomerByIdAsync_Should_ReturnNull_WhenMissing()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var context = CreateContext(dbName);

            var handler = new CustomerHandler(context);
            var response = await handler.GetCustomerByIdAsync(new GetCustomerByIdRequest { Id = Guid.NewGuid() });

            Assert.NotNull(response);
            Assert.Null(response.Data);
        }

        [Fact]
        public async Task UpdateCustomerAsync_Should_UpdateExistingCustomer()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var context = CreateContext(dbName);

            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                Name = "Before",
                Email = "before@example.com",
                Phone = "9",
                BirthDate = new DateTime(1980, 1, 1)
            };
            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            var handler = new CustomerHandler(context);
            var request = new UpdateCustomerRequest
            {
                Id = customer.Id,
                Name = "After",
                Email = "after@example.com",
                Phone = "0",
                BirthDate = new DateTime(1990, 2, 2)
            };

            var response = await handler.UpdateCustomerAsync(request);

            Assert.NotNull(response);
            Assert.Null(response.Message);
            Assert.NotNull(response.Data);
            Assert.Equal(request.Name, response.Data.Name);
            Assert.Equal(request.Email, response.Data.Email);

            var persisted = await context.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == customer.Id);
            Assert.Equal(request.Name, persisted.Name);
            Assert.Equal(request.Email, persisted.Email);
        }

        [Fact]
        public async Task UpdateCustomerAsync_Should_ReturnNotFound_When_Missing()
        {
            var dbName = Guid.NewGuid().ToString();
            await using var context = CreateContext(dbName);

            var handler = new CustomerHandler(context);
            var response = await handler.UpdateCustomerAsync(new UpdateCustomerRequest { Id = Guid.NewGuid(), Name = "X", Email = "x@x", Phone = "1", BirthDate = DateTime.Today });

            Assert.NotNull(response);
            Assert.Null(response.Data);
            Assert.Equal("Customer not found.", response.Message);
        }

        [Fact]
        public async Task UpdateCustomerAsync_When_SaveChangesFails_ReturnsErrorResponse()
        {
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            // seed normally
            await using (var seed = new AppDbContext(options))
            {
                seed.Customers.Add(Customer);
                await seed.SaveChangesAsync();
            }

            await using var faulty = new FaultyDbContext(options);
            var existing = await faulty.Customers.AsNoTracking().FirstOrDefaultAsync();
            var handler = new CustomerHandler(faulty);

            var request = new UpdateCustomerRequest
            {
                Id = existing.Id,
                Name = "New",
                Email = "new@e",
                Phone = "0",
                BirthDate = DateTime.Today
            };

            var response = await handler.UpdateCustomerAsync(request);

            Assert.NotNull(response);
            Assert.Null(response.Data);
            Assert.False(string.IsNullOrEmpty(response.Message));
            Assert.False(response.IsSuccess);
        }
    }
}
