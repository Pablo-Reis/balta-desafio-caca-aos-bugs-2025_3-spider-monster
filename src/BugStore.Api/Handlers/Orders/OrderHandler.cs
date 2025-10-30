using BugStore.Data;
using BugStore.Handlers.Products;
using BugStore.Interfaces.Handlers;
using BugStore.Models;
using BugStore.Requests.Orders;
using BugStore.Requests.Products;
using BugStore.Responses;
using Microsoft.EntityFrameworkCore;

namespace BugStore.Handlers.Orders
{
    public class OrderHandler(AppDbContext context, IProductHandler productHandler) : IOrderHandler
    {
        public async Task<Response<Order>> CreateOrderAsync(CreateOrderRequest request)
        {
            var products = await productHandler.GetProductsAsync(new GetProductsRequest());

            var order = new Order
            {
                CustomerId = request.CustomerId,
                CreatedAt = DateTime.UtcNow,
                Lines = request.Lines.Select(line => new OrderLine
                {
                    ProductId = line.ProductId,
                    Quantity = line.Quantity,
                    Total = products.Data?.FirstOrDefault(x => x.Id == line.ProductId) is not null ? products.Data.FirstOrDefault(x => x.Id == line.ProductId).Price * line.Quantity : 0.00m
                }).ToList()
            };

            try
            {
                var createdModel = await context.Orders.AddAsync(order);
                await context.SaveChangesAsync();

                return new Response<Order>(createdModel.Entity);
            }

            catch (Exception e)
            {
                return new Response<Order>(null, message: "Ocorreu um erro ao cadastrar pedido.");
            }
            
        }

        public async Task<Response<Order>> GetOrderByIdAsync(GetOrderByIdRequest request)
        {
            
            try
            {
                var order = await context.Orders.AsNoTracking().Include(x => x.Customer).Include(x => x.Lines).ThenInclude(x => x.Product).FirstOrDefaultAsync(x => x.Id == request.Id);

                return order is not null ? new Response<Order>(order) : new Response<Order>(null, message: "Nenhum pedido encontrado.");
            }

            catch (Exception e)
            {
                return new Response<Order>(null, message: "Ocorreu um erro ao exibir cadastrar pedido.");
            }
        }
    }
}
