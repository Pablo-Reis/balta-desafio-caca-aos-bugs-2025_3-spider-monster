
using BugStore.Interfaces.Handlers;
using BugStore.Requests.Orders;
using BugStore.Responses;

namespace BugStore.Endpoints.Order
{
    public class CreateOrderEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/", handler: HandleAsync)
                .WithName("Order: Create")
                .WithSummary("Cria um pedido")
                .WithDescription("Cria um pedido")
                .WithOrder(1)
                .Produces<Response<Models.Order>>();

        }

        private static async Task<IResult> HandleAsync(
        IOrderHandler handler,
        CreateOrderRequest request)
        {
            var response = await handler.CreateOrderAsync(request);
            return response.IsSuccess
                ? TypedResults.Created($"v1/orders/{response.Data?.Id}", response)
                : TypedResults.BadRequest(response);
        }
    }
}
