using BugStore.Handlers.Customers;
using BugStore.Handlers.Orders;
using BugStore.Handlers.Products;
using BugStore.Interfaces.Handlers;

namespace BugStore.IoC
{
    public static class ApplicationModule
    {
        public static void AddApplicationModule(this IServiceCollection Services)
        {
            // Register application services here
            Services.AddScoped<ICustomerHandler, CustomerHandler>();
            Services.AddScoped<IProductHandler, ProductHandler>();
            Services.AddScoped<IOrderHandler, OrderHandler>();
            Services.AddSwaggerGen();
        }

        public static void AddDocumentation(this WebApplication app)
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
    }
}
