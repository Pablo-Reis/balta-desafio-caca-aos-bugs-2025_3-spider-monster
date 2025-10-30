
using BugStore.Interfaces.Handlers;
using BugStore.Models;
using BugStore.Requests.Customers;
using BugStore.Requests.Products;
using BugStore.Responses;
using Microsoft.Extensions.Configuration;

namespace BugStore.Endpoints.Product
{
    public class UpdateProductEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("/{id:Guid}", handler: HandleAsync)
                .WithName("Product: Update")
                .WithSummary("Atualiza um produto")
                .WithDescription("Atualiza um produto")
                .WithOrder(5)
                .Produces<Response<Models.Product>>();

        }

        private static async Task<IResult> HandleAsync(
        IProductHandler handler,
        UpdateProductRequest request,
        Guid id)
        {
            request.Id = id;
            var response = await handler.UpdateProductAsync(request);
            return response.IsSuccess
                ? TypedResults.Ok(response)
                : TypedResults.BadRequest(response);
        }
    }
}
