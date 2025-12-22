using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Application
{
    public static class ServiceExtension
    {

        public static void AddApplicationServices(this IServiceCollection services)
        {
            services.AddAutoMapper(cfg => { }, typeof(ServiceExtension).Assembly);
            // Registrar servicios de la aplicación aquí
            // Ejemplo: services.AddTransient<IMyService, MyService>();
        }


    }
}
