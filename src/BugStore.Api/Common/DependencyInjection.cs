using BugStore.IoC;

namespace BugStore.Common
{
    public static class DependencyInjection
    {
        public static void AddDependecyInjection(this WebApplicationBuilder builder)
        {
            // Register common services here
            builder.Services.AddInfraestructureModule(builder.Configuration);
            builder.Services.AddApplicationModule();
        }
    }
}
