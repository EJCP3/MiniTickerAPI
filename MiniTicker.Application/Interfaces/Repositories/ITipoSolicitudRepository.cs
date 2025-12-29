using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MiniTicker.Core.Domain.Entities;

namespace MiniTicker.Core.Application.Interfaces.Repositories
{
    public interface ITipoSolicitudRepository
    {
        Task AddAsync(TipoSolicitud tipoSolicitud);
        Task UpdateAsync(TipoSolicitud tipoSolicitud);
        Task DeleteAsync(TipoSolicitud tipoSolicitud);
        Task<TipoSolicitud?> GetByIdAsync(Guid id);
        Task<IReadOnlyList<TipoSolicitud>> GetByAreaIdAsync(Guid areaId);

        Task<IReadOnlyList<TipoSolicitud>> GetAllAsync(bool incluirInactivos = false);
    }
}