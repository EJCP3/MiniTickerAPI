using Microsoft.Extensions.DependencyInjection;
using AutoMapper;

namespace MiniTicker.Core.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddAutoMapper(cfg => { cfg.AddMaps(typeof(DependencyInjection)); });
            return services;
        }
    }
}
