using BugStore.Endpoints.Customer;
using BugStore.Endpoints.Order;
using BugStore.Endpoints.Product;

namespace BugStore.Endpoints
{
    public static class Endpoint
    {
        public static void MapEndpoints(this WebApplication app)
        {
            var endpoints = app.MapGroup("");

            endpoints.MapGroup("/")
            .WithTags("Health Check")
            .MapGet("/", () => new { message = "OK" });

            endpoints.MapGroup("v1/customers")
                .WithTags("Customers")
                .MapEndpoint<GetCustomersEndpoint>()
                .MapEndpoint<GetCustomerByIdEndpoint>()
                .MapEndpoint<CreateCustomerEndpoint>()
                .MapEndpoint<UpdateCustomerEndpoint>()
                .MapEndpoint<DeleteCustomerEndpoint>();

            endpoints.MapGroup("v1/products")
                .WithTags("Products")
                .MapEndpoint<GetProductsEndpoint>()
                .MapEndpoint<GetProductByIdEndpoint>()
                .MapEndpoint<CreateProductEndpoint>()
                .MapEndpoint<UpdateProductEndpoint>()
                .MapEndpoint<DeleteProductEndpoint>();

            endpoints.MapGroup("v1/orders")
                .WithTags("Orders")
                .MapEndpoint<GetOrderByIdEndpoint>()
                .MapEndpoint<CreateOrderEndpoint>();
        }

        private static IEndpointRouteBuilder MapEndpoint<TEndpoint>(this IEndpointRouteBuilder app)
        where TEndpoint : IEndpoint
        {
            TEndpoint.Map(app);
            return app;
        }
    }
    
    }
