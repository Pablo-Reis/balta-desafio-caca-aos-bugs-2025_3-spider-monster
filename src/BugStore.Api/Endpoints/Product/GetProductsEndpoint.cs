
using BugStore.Interfaces.Handlers;
using BugStore.Models;
using BugStore.Requests.Customers;
using BugStore.Requests.Products;
using BugStore.Responses;
using Microsoft.Extensions.Configuration;

namespace BugStore.Endpoints.Product
{
    public class GetProductsEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/", handler: HandleAsync)
                .WithName("Product: Get All")
                .WithSummary("Busca todos produtos")
                .WithDescription("Busca todos produtos")
                .WithOrder(3)
                .Produces<Response<Models.Product>>();
        }

        private static async Task<IResult> HandleAsync(
        IProductHandler handler)
        {
            var request = new GetProductsRequest();
            var response = await handler.GetProductsAsync(request);
            return response.IsSuccess
                ? TypedResults.Ok(response)
                : TypedResults.BadRequest(response);
        }
    }
}
