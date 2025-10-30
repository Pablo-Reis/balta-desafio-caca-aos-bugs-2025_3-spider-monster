
using BugStore.Interfaces.Handlers;
using BugStore.Models;
using BugStore.Requests.Customers;
using BugStore.Responses;
using Microsoft.Extensions.Configuration;

namespace BugStore.Endpoints.Customer
{
    public class DeleteCustomerEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
        {
            app.MapDelete("/{id:Guid}", handler: HandleAsync)
                .WithName("Customer: Delete")
                .WithSummary("Deleta um cliente")
                .WithDescription("Deleta um cliente")
                .WithOrder(3)
                .Produces<Response<Models.Customer>>();

        }

        private static async Task<IResult> HandleAsync(
        ICustomerHandler handler,
        Guid id)
        {
            var request = new DeleteCustomerRequest
            {
                Id = id
            };
            var response = await handler.DeleteCustomerAsync(request);
            return response.IsSuccess
                ? TypedResults.Ok(response)
                : TypedResults.BadRequest(response);
        }
    }
}
