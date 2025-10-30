
using BugStore.Interfaces.Handlers;
using BugStore.Models;
using BugStore.Requests.Customers;
using BugStore.Requests.Products;
using BugStore.Responses;
using Microsoft.Extensions.Configuration;

namespace BugStore.Endpoints.Product
{
    public class GetProductByIdEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/{id:Guid}", handler: HandleAsync)
                .WithName("Product: Get By Id")
                .WithSummary("Busca um produto")
                .WithDescription("Busca um produto")
                .WithOrder(4)
                .Produces<Response<Models.Product>>();
        }

        private static async Task<IResult> HandleAsync(
        IProductHandler handler,
        Guid id)
        {
            var request = new GetProductByIdRequest
            {
                Id = id
            };
            var response = await handler.GetProductByIdAsync(request);
            return response.IsSuccess
                ? TypedResults.Ok(response)
                : TypedResults.BadRequest(response);
        }
    }
}
