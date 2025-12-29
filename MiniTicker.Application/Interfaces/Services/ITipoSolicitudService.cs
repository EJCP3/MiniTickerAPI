using MiniTicker.Core.Application.Catalogs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MiniTicker.Core.Application.Interfaces.Services
{
    public interface ITipoSolicitudService
    {
        // ARREGLO CS0121: Un solo método GetAllAsync que maneja todo
        Task<IReadOnlyList<TipoSolicitudDto>> GetAllAsync(Guid? areaId = null, bool incluirInactivos = false, CancellationToken cancellationToken = default);

        Task<TipoSolicitudDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<TipoSolicitudDto> CreateAsync(TipoSolicitudDto dto, CancellationToken cancellationToken = default);
        Task<TipoSolicitudDto> UpdateAsync(Guid id, TipoSolicitudDto dto, CancellationToken cancellationToken = default);

        Task ActivateAsync(Guid id, CancellationToken cancellationToken = default);
        Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default);

        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

        // Puedes borrar GetByAreaIdAsync de la interfaz si ya usas GetAllAsync con filtro
    }
}