using Microsoft.EntityFrameworkCore;
using MiniTicker.Core.Domain.Entities;
using MiniTicker.Core.Domain.Enums;
using BCrypt.Net;

namespace MiniTicker.Infrastructure.Persistence.Seeds
{
    public static class UserSeed
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (await context.Usuarios.AnyAsync()) return;

            var areaTI = await context.Areas.FirstOrDefaultAsync(a => a.Nombre == "TI");
            if (areaTI == null) return;

            string passwordPlano = "123456";
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(passwordPlano);

            var usuarios = new List<Usuario>
            {
                 new Usuario
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Pedro",
                    Email = "Pedro@miniticker.com",
                    PasswordHash = passwordHash,
                    Rol = Rol.SuperAdmin,
                    Activo = true
                },

                new Usuario
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Luis",
                    Email = "Luis@miniticker.com",
                    PasswordHash = passwordHash,
                    Rol = Rol.Admin,
                    Activo = true
                },
                new Usuario
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Juan",
                    Email = "juan@miniticker.com",
                    PasswordHash = passwordHash,
                    Rol = Rol.Gestor,
                    AreaId = areaTI.Id,
                    Activo = true
                },
                new Usuario
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Ana",
                    Email = "ana@miniticker.com",
                    PasswordHash = passwordHash,
                    Rol = Rol.Solicitante,
                    Activo = true
                }
            };

            context.Usuarios.AddRange(usuarios);
            await context.SaveChangesAsync();
        }
    }
}