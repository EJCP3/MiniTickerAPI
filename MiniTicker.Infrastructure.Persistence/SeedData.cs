using Microsoft.EntityFrameworkCore;
using MiniTicker.Infrastructure.Persistence.Seeds;
using System; // Agrega esto

namespace MiniTicker.Infrastructure.Persistence
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            try
            {
            
                await context.Database.MigrateAsync();

              
                await CatalogSeed.SeedAsync(context);

             
                await UserSeed.SeedAsync(context);

            
                await TicketSeed.SeedAsync(context);

              
            }
            catch (Exception ex)
            {
                // Esto hará que el error salga en rojo en la consola de "Depuración"
                Console.WriteLine($"--> [ERROR FATAL] {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"--> [DETALLE INTERNO] {ex.InnerException.Message}");
                }
                throw; // Relanzamos para que se note en Program.cs
            }
        }
    }
}