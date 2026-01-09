using System;
using System.Collections.Generic;
using System.Linq; 
using System.Threading; 
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
                .Include(a => a.Tickets)
                .FirstOrDefaultAsync(a => a.Id == id)
                .ConfigureAwait(false);
        }

  public async Task<IReadOnlyList<Area>> GetAllAsync(bool incluirInactivos = false, CancellationToken cancellationToken = default)
{
    // ✅ CORRECCIÓN: Cambiamos 'var' por 'IQueryable<Area>'
    // Esto permite que podamos modificar la query con Where y OrderBy sin errores.
    IQueryable<Area> query = _context.Areas
        .AsNoTracking()
        .Include(a => a.Tickets); 

    // 2. Aplicamos el filtro
    if (incluirInactivos)
    {
        query = query.Where(t => t.Activo == false);
    }
    else
    {
        query = query.Where(t => t.Activo == true);
    }

    // 3. Ordenamos
    query = query.OrderBy(a => a.Nombre);

    // 4. Ejecutamos
    return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
}
}}