using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TuColmadoRD.Infrastructure.CrossCutting;
using TuColmadoRD.Infrastructure.Persistence;
namespace TuColmadoRD.Infrastructure.IOC.ServiceRegistrations
{
    public static class GlobalRegistrationsServices
    {
        public static void AddGlobalServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddCrossCuttingInfrastructure(configuration);
            services.AddLocalTenancy(configuration);
            services.AddPersistenceInfrastructure(configuration);
        }
    }
}
