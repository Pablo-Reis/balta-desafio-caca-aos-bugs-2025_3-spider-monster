
using BugStore.Interfaces.Handlers;
using BugStore.Requests.Orders;
using BugStore.Responses;

namespace BugStore.Endpoints.Order
{
    public class GetOrderByIdEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/{id:Guid}", handler: HandleAsync)
                .WithName("Order: Get by Id")
                .WithSummary("Busca um pedido")
                .WithDescription("Busca um pedido")
                .WithOrder(2)
                .Produces<Response<Models.Order>>();

        }

        private static async Task<IResult> HandleAsync(
        IOrderHandler handler,
        Guid id)
        {
            var request = new GetOrderByIdRequest { Id = id };
            var response = await handler.GetOrderByIdAsync(request);
            return response.IsSuccess
                ? TypedResults.Ok(response)
                : TypedResults.BadRequest(response);
        }
    }
}
