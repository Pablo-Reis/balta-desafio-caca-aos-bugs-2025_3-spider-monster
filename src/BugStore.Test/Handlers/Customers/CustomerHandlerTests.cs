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
using BugStore.Test.Data;

namespace BugStore.Test.Handlers.Customers
{
    public class CustomerHandlerTests
    {
        private Customer Customer { get; set; } = new Customer { Id = Guid.NewGuid(), Name = "New teste", Email = "teste@gmail.com", Phone = "123456789", BirthDate = DateTime.Today.AddYears(-18) };
        private static AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task CreateCustomerAsync_Should_CreateCustomer()
        {
            //Arrange
            await using var context = CreateContext();
            var handler = new CustomerHandler(context);
            var request = new CreateCustomerRequest
            {
                Name = "Teste Novo",
                Email = "novoteste@gmail.com",
                Phone = "123456789",
                BirthDate = new DateTime(1990, 1, 1)
            };

            //Act
            var response = await handler.CreateCustomerAsync(request);
            var persisted = await context.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == response.Data.Id);

            //Assert
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
            Assert.NotNull(persisted);
            Assert.Equal(request.Name, persisted.Name);
            Assert.Equal(request.Email, persisted.Email);
            Assert.Equal(request.Phone, persisted.Phone);
            Assert.Equal(request.BirthDate, persisted.BirthDate);
        }

        [Fact]
        public async Task CreateCustomerAsync_When_SaveChangesFails_ReturnsErrorResponse()
        {
            //Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
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

            //Act
            var response = await handler.CreateCustomerAsync(request);

            //Assert
            Assert.NotNull(response);
            Assert.Null(response.Data);
            Assert.False(string.IsNullOrWhiteSpace(response.Message));
            Assert.False(response.IsSuccess);
        }

        [Fact]
        public async Task DeleteCustomerAsync_Should_DeleteExistingCustomer()
        {
            //Arrange
            await using var context = CreateContext();
            var handler = new CustomerHandler(context);
            Customer.Id = Guid.NewGuid();


            //Act
            await context.Customers.AddAsync(Customer, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
            var response = await handler.DeleteCustomerAsync(new DeleteCustomerRequest { Id = Customer.Id });

            //Assert
            Assert.NotNull(response);
            Assert.Null(response.Message);
            Assert.NotNull(response.Data);
            Assert.Equal(Customer.Id, response.Data.Id);
            Assert.False(context.Customers.Any(c => c.Id == Customer.Id));
        }

        [Fact]
        public async Task DeleteCustomerAsync_Should_ReturnNotFound_When_Missing()
        {
            //Arrange
            await using var context = CreateContext();
            var handler = new CustomerHandler(context);

            //Act
            var response = await handler.DeleteCustomerAsync(new DeleteCustomerRequest { Id = Guid.NewGuid() });

            //Assert
            Assert.NotNull(response);
            Assert.Null(response.Data);
            Assert.False(string.IsNullOrEmpty(response.Message));
            Assert.False(response.IsSuccess);
        }

        [Fact]
        public async Task DeleteCustomerAsync_When_SaveChangesFails_Should_ReturnsErrorResponse()
        {
            //Arrange
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            await using (var context = new AppDbContext(options))
            {
                context.Customers.Add(Customer);
                await context.SaveChangesAsync();
            }

            await using var faulty = new FaultyDbContext(options);
            var toDelete = await faulty.Customers.AsNoTracking().FirstOrDefaultAsync();
            var handler = new CustomerHandler(faulty);

            //Act
            var response = await handler.DeleteCustomerAsync(new DeleteCustomerRequest { Id = toDelete.Id });

            //Assert
            Assert.NotNull(response);
            Assert.Null(response.Data);
            Assert.False(string.IsNullOrEmpty(response.Message));
            Assert.False(response.IsSuccess);
        }

        [Fact]
        public async Task GetCustomerAsync_Should_ReturnAllCustomers()
        {
            //Arrange
            await using var context = CreateContext();
            context.Customers.AddRange(new[]
            {
                new Customer { Id = Guid.NewGuid(), Name = "Test 1", Email = "testOne@gmail.com", Phone = "123455332", BirthDate = DateTime.Today.AddYears(-20) },
                new Customer { Id = Guid.NewGuid(), Name = "Teste 2", Email = "testTwo@gmail.com", Phone = "234243232", BirthDate = DateTime.Today.AddYears(-23) }
            });
            await context.SaveChangesAsync();
            var handler = new CustomerHandler(context);

            //Act
            var response = await handler.GetCustomerAsync(new GetCustomersRequest());

            //Assert
            Assert.NotNull(response);
            Assert.Null(response.Message);
            Assert.NotNull(response.Data);
            Assert.Equal(2, response.Data.Count);
            Assert.True(response.IsSuccess);
        }

        [Fact]
        public async Task GetCustomerByIdAsync_Should_ReturnCustomer_WhenExists()
        {
            //Arrange
            Customer.Id = Guid.NewGuid();
            await using var context = CreateContext();
            var handler = new CustomerHandler(context);
            
            //Act
            context.Customers.Add(Customer);
            await context.SaveChangesAsync();
            var response = await handler.GetCustomerByIdAsync(new GetCustomerByIdRequest { Id = Customer.Id });

            //Assert
            Assert.NotNull(response);
            Assert.Null(response.Message);
            Assert.NotNull(response.Data);
            Assert.Equal(Customer.Id, response.Data.Id);
            Assert.True(response.IsSuccess);
        }

        [Fact]
        public async Task GetCustomerByIdAsync_Should_ReturnNull_WhenMissing()
        {
            //Arrange
            var dbName = Guid.NewGuid().ToString();
            await using var context = CreateContext();
            var handler = new CustomerHandler(context);

            //Act
            var response = await handler.GetCustomerByIdAsync(new GetCustomerByIdRequest { Id = Guid.NewGuid() });

            //Assert
            Assert.NotNull(response);
            Assert.Null(response.Data);
        }

        [Fact]
        public async Task UpdateCustomerAsync_Should_UpdateExistingCustomer()
        {
            //Arrange
            Customer.Id = Guid.NewGuid();
            await using var context = CreateContext();
            var handler = new CustomerHandler(context);
            var request = new UpdateCustomerRequest
            {
                Id = Customer.Id,
                Name = "After",
                Email = "after@gmail.com",
                Phone = "3113131130",
                BirthDate = new DateTime(1990, 2, 2)
            };

            //Act
            context.Customers.Add(Customer);
            await context.SaveChangesAsync();
            var response = await handler.UpdateCustomerAsync(request);
            var persisted = await context.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == Customer.Id);

            //Assert
            Assert.NotNull(response);
            Assert.Null(response.Message);
            Assert.NotNull(response.Data);
            Assert.True(response.IsSuccess);
            Assert.Equal(request.Name, response.Data.Name);
            Assert.Equal(request.Email, response.Data.Email);
            Assert.Equal(request.Phone, response.Data.Phone);
            Assert.Equal(request.BirthDate, response.Data.BirthDate);


            Assert.Equal(request.Name, persisted.Name);
            Assert.Equal(request.Email, persisted.Email);
            Assert.Equal(request.Phone, persisted.Phone);
            Assert.Equal(request.BirthDate, persisted.BirthDate);
        }

        [Fact]
        public async Task UpdateCustomerAsync_Should_ReturnNotFound_When_Missing()
        {
            //Arrange
            await using var context = CreateContext();
            var handler = new CustomerHandler(context);

            //Act
            var response = await handler.UpdateCustomerAsync(new UpdateCustomerRequest { Id = Guid.NewGuid(), Name = "X", Email = "x@x", Phone = "1", BirthDate = DateTime.Today });

            //Assert
            Assert.NotNull(response);
            Assert.Null(response.Data);
            Assert.False(string.IsNullOrEmpty(response.Message));
            Assert.False(response.IsSuccess);

        }

        [Fact]
        public async Task UpdateCustomerAsync_When_SaveChangesFails_ReturnsErrorResponse()
        {
            //Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            await using var faulty = new FaultyDbContext(options);
            var handler = new CustomerHandler(faulty);
            await using (var seed = new AppDbContext(options))
            {
                seed.Customers.Add(Customer);
                await seed.SaveChangesAsync();
            }

            var request = new UpdateCustomerRequest
            {
                Id = Customer.Id,
                Name = "New",
                Email = "new@e",
                Phone = "0",
                BirthDate = DateTime.Today
            };

            //Act
            var response = await handler.UpdateCustomerAsync(request);

            //Assert
            Assert.NotNull(response);
            Assert.Null(response.Data);
            Assert.False(string.IsNullOrEmpty(response.Message));
            Assert.False(response.IsSuccess);
        }
    }
}
