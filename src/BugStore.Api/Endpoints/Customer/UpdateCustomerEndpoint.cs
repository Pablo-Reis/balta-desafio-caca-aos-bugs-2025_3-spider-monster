
using BugStore.Interfaces.Handlers;
using BugStore.Models;
using BugStore.Requests.Customers;
using BugStore.Responses;
using Microsoft.Extensions.Configuration;

namespace BugStore.Endpoints.Customer
{
    public class UpdateCustomerEndpoint : IEndpoint
    {
        public static void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("/{id:Guid}", handler: HandleAsync)
                .WithName("Customer: Update")
                .WithSummary("Atualiza um cliente")
                .WithDescription("Atualiza um cliente")
                .WithOrder(2)
                .Produces<Response<Models.Customer>>();

        }

        private static async Task<IResult> HandleAsync(
        ICustomerHandler handler,
        UpdateCustomerRequest request,
        Guid id)
        {
            request.Id = id;
            var response = await handler.UpdateCustomerAsync(request);
            return response.IsSuccess
                ? TypedResults.Ok(response)
                : TypedResults.BadRequest(response);
        }
    }
}
