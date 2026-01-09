using Microsoft.EntityFrameworkCore;
using MiniTicker.Core.Domain.Entities;
using MiniTicker.Core.Domain.Enums;
using MiniTicker.Infrastructure.Persistence; 
using BCrypt.Net;

namespace MiniTicker.Infrastructure.Persistence.Seeds
{
    public static class UserSeed
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
           
            var areaTI = await context.Areas.FirstOrDefaultAsync(a => a.Nombre == "TI");

         
            string passwordPlano = "123456";
         
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(passwordPlano);

            var listaUsuarios = new List<Usuario>
            {
                // SuperAdmin
                new Usuario
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Pedro",
                    Email = "Pedro@miniticker.com",
                    PasswordHash = passwordHash,
                    Rol = Rol.SuperAdmin,
                    Activo = true
                },
                // Admin
                new Usuario
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Luis",
                    Email = "Luis@miniticker.com",
                    PasswordHash = passwordHash,
                    Rol = Rol.Admin,
                    Activo = true
                },
                // Solicitante
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

            // Agregamos al Gestor (Juan) SOLO si el área TI existe
            if (areaTI != null)
            {
                listaUsuarios.Add(new Usuario
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Juan",
                    Email = "juan@miniticker.com",
                    PasswordHash = passwordHash,
                    Rol = Rol.Gestor,
                    AreaId = areaTI.Id, 
                    Activo = true
                });
            }

            // 5. LÓGICA DE UPSERT (Insertar si no existe)
            foreach (var usuario in listaUsuarios)
            {
                // Verificamos por Email para no duplicar
                bool existe = await context.Usuarios.AnyAsync(u => u.Email == usuario.Email);

                if (!existe)
                {
                    context.Usuarios.Add(usuario);
                }
            }

            await context.SaveChangesAsync();
        }
    }
}