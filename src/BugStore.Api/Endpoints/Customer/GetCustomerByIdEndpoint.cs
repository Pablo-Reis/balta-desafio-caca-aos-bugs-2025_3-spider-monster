
using BugStore.Interfaces.Handlers;
using BugStore.Models;
using BugStore.Requests.Customers;
using BugStore.Responses;
using Microsoft.Extensions.Configuration;

namespace BugStore.Endpoints.Customer
{
    public class GetCustomerByIdEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/{id:Guid}", handler: HandleAsync)
                .WithName("Customer: Get By Id")
                .WithSummary("Busca um cliente")
                .WithDescription("Busca um cliente")
                .WithOrder(4)
                .Produces<Response<Models.Customer>>();

        }

        private static async Task<IResult> HandleAsync(
        ICustomerHandler handler,
        Guid id)
        {
            var request = new GetCustomerByIdRequest
            {
                Id = id
            };
            var response = await handler.GetCustomerByIdAsync(request);
            return response.IsSuccess
                ? TypedResults.Ok(response)
                : TypedResults.BadRequest(response);
        }
    }
}
