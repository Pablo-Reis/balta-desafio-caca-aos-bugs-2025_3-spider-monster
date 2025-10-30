using BugStore.Data;
using Microsoft.EntityFrameworkCore;

namespace BugStore.IoC
{
    public static class InfraestructureModule
    {
        public static void AddInfraestructureModule(this IServiceCollection Services, IConfiguration configuration)
        {
            Services.AddDbContext<AppDbContext>(x => x.UseSqlite(configuration.GetConnectionString("DefaultConnection")));
        }
    }
}
