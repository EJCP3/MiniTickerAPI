using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MiniTicker.Core.Application.Interfaces.Repositories;
using MiniTicker.Core.Application.Interfaces.Services;
using MiniTicker.Core.Application.Services;
using MiniTicker.Infrastructure.Persistence.Repositories;
using MiniTicker.Infrastructure.Persistence.Services;

namespace MiniTicker.Infrastructure.Persistence
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection")));

            // =========================
            // Repositories
            // =========================
            services.AddScoped<IAreaRepository, AreaRepository>();
            services.AddScoped<ITicketRepository, TicketRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ITipoSolicitudRepository, TipoSolicitudRepository>();
            services.AddScoped<IComentarioRepository, ComentarioRepository>();
            services.AddScoped<ITicketEventRepository, TicketEventRepository>(); 

            // =========================
            // Application Services
            // =========================
            services.AddScoped<IAreaService, AreaService>();
            services.AddScoped<ITicketService, TicketService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ITipoSolicitudService, TipoSolicitudService>();
            services.AddScoped<IAuthService, AuthService>();

            // ✅ AQUÍ AGREGAS EL NUEVO SERVICIO:
            services.AddScoped<IActivityService, ActivityService>();

            // =========================
            // Technical Services
            // =========================
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IFileStorageService, FileStorageService>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<ISystemEventRepository, SystemEventRepository>();
            return services;
        }
    }
}