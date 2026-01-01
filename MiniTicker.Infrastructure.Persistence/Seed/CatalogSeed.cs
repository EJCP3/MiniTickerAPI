using Microsoft.EntityFrameworkCore;
using MiniTicker.Core.Domain.Entities;
using MiniTicker.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniTicker.Infrastructure.Persistence.Seeds
{
    public static class CatalogSeed
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // =================================================================
            // PASO 1: ASEGURAR QUE LAS ÁREAS EXISTAN (Y TENGAN PREFIJO)
            // =================================================================

            // --- Área TI ---
            var areaTI = await context.Areas.FirstOrDefaultAsync(a => a.Nombre == "TI");
            if (areaTI == null)
            {
                areaTI = new Area { Id = Guid.NewGuid(), Nombre = "TI", Prefijo = "TEC", Activo = true };
                context.Areas.Add(areaTI);
            }
            else if (string.IsNullOrEmpty(areaTI.Prefijo)) // Si existe pero no tiene prefijo, se lo ponemos
            {
                areaTI.Prefijo = "TEC";
            }

            // --- Área Mantenimiento ---
            var areaMant = await context.Areas.FirstOrDefaultAsync(a => a.Nombre == "Mantenimiento");
            if (areaMant == null)
            {
                areaMant = new Area { Id = Guid.NewGuid(), Nombre = "Mantenimiento", Prefijo = "MAN", Activo = true };
                context.Areas.Add(areaMant);
            }
            else if (string.IsNullOrEmpty(areaMant.Prefijo))
            {
                areaMant.Prefijo = "MAN";
            }

            // --- Área Transporte ---
            var areaTrans = await context.Areas.FirstOrDefaultAsync(a => a.Nombre == "Transporte");
            if (areaTrans == null)
            {
                areaTrans = new Area { Id = Guid.NewGuid(), Nombre = "Transporte", Prefijo = "TRA", Activo = true };
                context.Areas.Add(areaTrans);
            }
            else if (string.IsNullOrEmpty(areaTrans.Prefijo))
            {
                areaTrans.Prefijo = "TRA";
            }

            // --- Área Compras ---
            var areaComp = await context.Areas.FirstOrDefaultAsync(a => a.Nombre == "Compras");
            if (areaComp == null)
            {
                areaComp = new Area { Id = Guid.NewGuid(), Nombre = "Compras", Prefijo = "COM", Activo = true };
                context.Areas.Add(areaComp);
            }
            else if (string.IsNullOrEmpty(areaComp.Prefijo))
            {
                areaComp.Prefijo = "COM";
            }

            // GUARDAMOS CAMBIOS AHORA PARA LOS PREFIJOS
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
                var existe = await context.TiposSolicitud
                    .AnyAsync(t => t.Nombre == item.Nombre && t.AreaId == item.AreaId);

                if (!existe)
                {
                    item.Id = Guid.NewGuid();
                    context.TiposSolicitud.Add(item);
                }
            }

            // Guardamos los Tipos
            await context.SaveChangesAsync();
        }
    }
}