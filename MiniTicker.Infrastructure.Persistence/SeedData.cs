using MiniTicker.Infrastructure.Persistence.Seeds; // Importante para ver las clases staticas

namespace MiniTicker.Infrastructure.Persistence
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            await context.Database.EnsureCreatedAsync();

            // 1. Primero Catálogos (No dependen de nadie)
            await CatalogSeed.SeedAsync(context);

            // 2. Usuarios (Dependen de Áreas)
            await UserSeed.SeedAsync(context);

            // 3. Tickets (Dependen de Usuarios y Catálogos)
            await TicketSeed.SeedAsync(context);
        }
    }
}