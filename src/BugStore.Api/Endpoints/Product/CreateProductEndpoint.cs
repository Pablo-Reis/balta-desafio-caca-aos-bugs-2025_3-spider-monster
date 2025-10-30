
using BugStore.Interfaces.Handlers;
using BugStore.Requests.Products;
using BugStore.Responses;

namespace BugStore.Endpoints.Product
{
    public class CreateProductEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/", handler: HandleAsync)
                .WithName("Product: Create")
                .WithSummary("Cria um produto")
                .WithDescription("Cria um produto")
                .WithOrder(1)
                .Produces<Response<Models.Product>>();

        }

        private static async Task<IResult> HandleAsync(
        IProductHandler handler,
        CreateProductRequest request)
        {
            var response = await handler.CreateProductAsync(request);
            return response.IsSuccess
                ? TypedResults.Created($"v1/products/{response.Data?.Id}", response)
                : TypedResults.BadRequest(response);
        }
    }
}
