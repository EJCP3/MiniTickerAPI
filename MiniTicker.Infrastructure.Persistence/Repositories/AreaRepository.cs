using System;
using System.Collections.Generic;
using System.Linq; // Necesario para Where y OrderBy
using System.Threading; // Necesario para CancellationToken
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MiniTicker.Infrastructure.Persistence;
using MiniTicker.Core.Application.Interfaces.Repositories;
using MiniTicker.Core.Domain.Entities;

namespace MiniTicker.Infrastructure.Persistence.Repositories
{
    internal class AreaRepository : IAreaRepository
    {
        private readonly ApplicationDbContext _context;

        public AreaRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task AddAsync(Area area)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            await _context.Areas.AddAsync(area).ConfigureAwait(false);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task UpdateAsync(Area area)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            _context.Areas.Update(area);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task DeleteAsync(Area area)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            _context.Areas.Remove(area);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<Area?> GetByIdAsync(Guid id)
        {
            return await _context.Areas
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id)
                .ConfigureAwait(false);
        }

        // ✅ Corrección CS0535: Agregamos 'CancellationToken cancellationToken = default'
        public async Task<IReadOnlyList<Area>> GetAllAsync(bool incluirInactivos = false, CancellationToken cancellationToken = default)
        {
            // 1. Preparamos la consulta base
            var query = _context.Areas.AsNoTracking();

            // 2. Aplicamos el filtro:
            // Si NO queremos ver inactivos (!incluirInactivos), filtramos solo los Activos.
            if (incluirInactivos)
            {
                query = query.Where(t => t.Activo == false); // Modo Papelera
            }
            else
            {
                query = query.Where(t => t.Activo == true); // Modo Normal
            }


            // 3. Ordenamos por nombre para que la lista se vea ordenada
            query = query.OrderBy(a => a.Nombre);

            // 4. Ejecutamos pasando el token
            return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}