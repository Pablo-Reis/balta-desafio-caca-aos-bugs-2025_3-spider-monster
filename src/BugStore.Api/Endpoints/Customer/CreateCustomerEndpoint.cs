
using BugStore.Interfaces.Handlers;
using BugStore.Models;
using BugStore.Requests.Customers;
using BugStore.Responses;
using Microsoft.Extensions.Configuration;

namespace BugStore.Endpoints.Customer
{
    public class CreateCustomerEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/", handler: HandleAsync)
                .WithName("Customer: Create")
                .WithSummary("Cria um cliente")
                .WithDescription("Cria um cliente")
                .WithOrder(1)
                .Produces<Response<Models.Customer>>();

        }

        private static async Task<IResult> HandleAsync(
        ICustomerHandler handler,
        CreateCustomerRequest request)
        {
            var response = await handler.CreateCustomerAsync(request);
            return response.IsSuccess
                ? TypedResults.Created($"v1/categories/{response.Data?.Id}", response)
                : TypedResults.BadRequest(response);
        }
    }
}
