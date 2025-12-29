using Microsoft.EntityFrameworkCore;
using MiniTicker.Core.Domain.Entities;
// Ajusta este namespace al de tu DbContext
using MiniTicker.Infrastructure.Persistence;

namespace MiniTicker.Infrastructure.Persistence.Seeds
{
    public static class CatalogSeed
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // =================================================================
            // PASO 1: ASEGURAR QUE LAS ÁREAS EXISTAN
            // =================================================================

            // --- Área TI ---
            var areaTI = await context.Areas.FirstOrDefaultAsync(a => a.Nombre == "TI");
            if (areaTI == null)
            {
                areaTI = new Area { Id = Guid.NewGuid(), Nombre = "TI", Activo = true };
                context.Areas.Add(areaTI);
            }

            // --- Área Mantenimiento ---
            var areaMant = await context.Areas.FirstOrDefaultAsync(a => a.Nombre == "Mantenimiento");
            if (areaMant == null)
            {
                areaMant = new Area { Id = Guid.NewGuid(), Nombre = "Mantenimiento", Activo = true };
                context.Areas.Add(areaMant);
            }

            // --- Área Transporte ---
            var areaTrans = await context.Areas.FirstOrDefaultAsync(a => a.Nombre == "Transporte");
            if (areaTrans == null)
            {
                areaTrans = new Area { Id = Guid.NewGuid(), Nombre = "Transporte", Activo = true };
                context.Areas.Add(areaTrans);
            }

            // --- Área Compras ---
            var areaComp = await context.Areas.FirstOrDefaultAsync(a => a.Nombre == "Compras");
            if (areaComp == null)
            {
                areaComp = new Area { Id = Guid.NewGuid(), Nombre = "Compras", Activo = true };
                context.Areas.Add(areaComp);
            }

            // GUARDAMOS CAMBIOS AHORA. 
            // Esto es crucial para asegurar que las Áreas tengan ID antes de usarlas en los Tipos.
            await context.SaveChangesAsync();


            // =================================================================
            // PASO 2: ASEGURAR QUE LOS TIPOS DE SOLICITUD EXISTAN
            // =================================================================

            // Definimos una lista de lo que queremos crear
            var tiposParaCrear = new List<TipoSolicitud>
            {
                // TI
                new TipoSolicitud { Nombre = "Soporte PC", AreaId = areaTI.Id, Activo = true },
                new TipoSolicitud { Nombre = "Acceso a Sistemas", AreaId = areaTI.Id, Activo = true },
                new TipoSolicitud { Nombre = "Instalación Software", AreaId = areaTI.Id, Activo = true },
                
                // Mantenimiento
                new TipoSolicitud { Nombre = "Reparación Aire Acondicionado", AreaId = areaMant.Id, Activo = true },
                new TipoSolicitud { Nombre = "Fontanería", AreaId = areaMant.Id, Activo = true },
                new TipoSolicitud { Nombre = "Electricidad", AreaId = areaMant.Id, Activo = true },

                // Transporte
                new TipoSolicitud { Nombre = "Solicitud Vehículo", AreaId = areaTrans.Id, Activo = true },
                new TipoSolicitud { Nombre = "Mantenimiento Vehículo", AreaId = areaTrans.Id, Activo = true },

                // Compras
                new TipoSolicitud { Nombre = "Insumos Oficina", AreaId = areaComp.Id, Activo = true }
            };

            foreach (var item in tiposParaCrear)
            {
                // Verificamos si YA existe este tipo en esa área específica
                // (Usamos AnyAsync para no duplicar)
                var existe = await context.TiposSolicitud
                    .AnyAsync(t => t.Nombre == item.Nombre && t.AreaId == item.AreaId);

                if (!existe)
                {
                    // Si no existe, lo agregamos
                    item.Id = Guid.NewGuid();
                    context.TiposSolicitud.Add(item);
                }
            }

            // Guardamos los Tipos
            await context.SaveChangesAsync();
        }
    }
}