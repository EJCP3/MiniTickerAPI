using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MiniTicker.Core.Application.Interfaces.Repositories;
using MiniTicker.Core.Application.Interfaces.Services;
using MiniTicker.Infrastructure.Persistence;
using MiniTicker.Infrastructure.Persistence.Repositories;
using MiniTicker.Infrastructure.Persistence.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IAreaRepository, AreaRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITipoSolicitudRepository, TipoSolicitudRepository>();
        services.AddScoped<IComentarioRepository, ComentarioRepository>();

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IFileStorageService, FileStorageService>();

        return services;
    }
}
