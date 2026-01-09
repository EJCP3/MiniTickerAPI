using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MiniTicker.Infrastructure.Persistence;
using MiniTicker.Core.Application.Interfaces.Repositories;
using MiniTicker.Core.Domain.Entities;

namespace MiniTicker.Infrastructure.Persistence.Repositories
{
    internal class TipoSolicitudRepository : ITipoSolicitudRepository
    {
        private readonly ApplicationDbContext _context;

        public TipoSolicitudRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task AddAsync(TipoSolicitud tipoSolicitud)
        {
            if (tipoSolicitud == null) throw new ArgumentNullException(nameof(tipoSolicitud));

            await _context.TiposSolicitud.AddAsync(tipoSolicitud).ConfigureAwait(false);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task UpdateAsync(TipoSolicitud tipoSolicitud)
        {
            if (tipoSolicitud == null) throw new ArgumentNullException(nameof(tipoSolicitud));

            _context.TiposSolicitud.Update(tipoSolicitud);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task DeleteAsync(TipoSolicitud tipoSolicitud)
        {
            if (tipoSolicitud == null) throw new ArgumentNullException(nameof(tipoSolicitud));

            _context.TiposSolicitud.Remove(tipoSolicitud);
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<TipoSolicitud?> GetByIdAsync(Guid id)
        {
            return await _context.TiposSolicitud
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id)
                .ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<TipoSolicitud>> GetInactivosAsync()
        {
            return await _context.TiposSolicitud
                .AsNoTracking()
                .Where(t => t.Activo == false) // <--- Solo los inactivos
                .ToListAsync();
        }

        public async Task<IReadOnlyList<TipoSolicitud>> GetAllAsync(bool incluirInactivos = false)
        {
            var query = _context.TiposSolicitud.AsNoTracking();

            if (incluirInactivos)
            {
                query = query.Where(t => t.Activo == false); // Modo Papelera
            }
            else
            {
                query = query.Where(t => t.Activo == true); // Modo Normal
            }

            query = query.OrderBy(a => a.Nombre);

            //if (!incluirInactivos)
            //{
            //    // Si incluirInactivos es FALSE, filtramos para ver SOLO los activos (true)
            //    query = query.Where(t => t.Activo == true);
            //}

            // Si incluirInactivos es TRUE, no filtramos nada y devolvemos todo (activos y borrados)

            return await query.ToListAsync().ConfigureAwait(false);
        }
        public async Task<IReadOnlyList<TipoSolicitud>> GetByAreaIdAsync(Guid areaId)
        {
            var query = _context.TiposSolicitud
                .AsNoTracking()
                .Where(t => t.Activo == true); // <--- FILTRO IMPORTANTE: Solo activos

            // Si quieres soportar que Guid.Empty traiga TODAS las áreas:
            if (areaId != Guid.Empty)
            {
                query = query.Where(t => t.AreaId == areaId);
            }

            return await query.ToListAsync().ConfigureAwait(false);
        }
    }
}
