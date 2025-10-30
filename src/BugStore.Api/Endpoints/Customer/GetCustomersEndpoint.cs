
using BugStore.Interfaces.Handlers;
using BugStore.Models;
using BugStore.Requests.Customers;
using BugStore.Responses;
using Microsoft.Extensions.Configuration;

namespace BugStore.Endpoints.Customer
{
    public class GetCustomersEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/", handler: HandleAsync)
                .WithName("Customer: Get All")
                .WithSummary("Busca todos os clientes")
                .WithDescription("Busca todos os clientes")
                .WithOrder(5)
                .Produces<Response<Models.Customer>>();

        }

        private static async Task<IResult> HandleAsync(
        ICustomerHandler handler)
        {
            var request = new GetCustomersRequest();
            var response = await handler.GetCustomerAsync(request);
            return response.IsSuccess
                ? TypedResults.Ok(response)
                : TypedResults.BadRequest(response);
        }
    }
}
