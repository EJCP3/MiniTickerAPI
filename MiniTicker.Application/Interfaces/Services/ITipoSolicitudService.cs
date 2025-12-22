using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MiniTicker.Core.Application.Catalogs;

namespace MiniTicker.Core.Application.Interfaces.Services
{
    /// <summary>
    /// Servicio de aplicación para la gestión de Tipos de Solicitud.
    /// Contrato sin implementación; pensado para uso por roles Admin/SuperAdmin.
    /// </summary>
    public interface ITipoSolicitudService
    {
        /// <summary>
        /// Obtiene todos los tipos de solicitud o los filtrados por <paramref name="areaId"/>.
        /// </summary>
        Task<IReadOnlyList<TipoSolicitudDto>> GetAllAsync(Guid? areaId = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene un tipo de solicitud por su identificador.
        /// Devuelve null si no existe.
        /// </summary>
        Task<TipoSolicitudDto?> GetByIdAsync(Guid tipoSolicitudId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Crea un nuevo tipo de solicitud y devuelve su DTO.
        /// </summary>
        Task<TipoSolicitudDto> CreateAsync(TipoSolicitudDto dto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Actualiza un tipo de solicitud existente y devuelve el DTO actualizado.
        /// </summary>
        Task<TipoSolicitudDto> UpdateAsync(Guid tipoSolicitudId, TipoSolicitudDto dto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Activa un tipo de solicitud.
        /// </summary>
        Task ActivateAsync(Guid tipoSolicitudId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Desactiva un tipo de solicitud.
        /// </summary>
        Task DeactivateAsync(Guid tipoSolicitudId, CancellationToken cancellationToken = default);
        Task GetByAreaIdAsync(Guid areaId);
        Task DeleteAsync(Guid id);
    }
}