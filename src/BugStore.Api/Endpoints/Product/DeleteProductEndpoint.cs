
using BugStore.Interfaces.Handlers;
using BugStore.Models;
using BugStore.Requests.Customers;
using BugStore.Requests.Products;
using BugStore.Responses;
using Microsoft.Extensions.Configuration;

namespace BugStore.Endpoints.Product
{
    public class DeleteProductEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
        {
            app.MapDelete("/{id:Guid}", handler: HandleAsync)
                .WithName("Product: Delete")
                .WithSummary("Deleta um produto")
                .WithDescription("Deleta um produto")
                .WithOrder(2)
                .Produces<Response<Models.Product>>();
        }

        private static async Task<IResult> HandleAsync(
        IProductHandler handler,
        Guid id)
        {
            var request = new DeleteProductRequest
            {
                Id = id
            };
            var response = await handler.DeleteProductAsync(request);
            return response.IsSuccess
                ? TypedResults.Ok(response)
                : TypedResults.BadRequest(response);
        }
    }
}
