using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MiniTicker.Core.Application.Catalogs;

namespace MiniTicker.Core.Application.Interfaces.Services
{
    /// <summary>
    /// Servicio de aplicación para la gestión de áreas.
    /// Solo contratos; diseñado para ser usado por roles Admin/SuperAdmin.
    /// </summary>
    public interface IAreaService
    {
        /// <summary>
        /// Obtiene todas las áreas.
        /// </summary>
        Task<IReadOnlyList<AreaDto>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene un área por su identificador.
        /// Devuelve null si no existe.
        /// </summary>
        Task<AreaDto?> GetByIdAsync(Guid areaId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Crea una nueva área y devuelve su DTO.
        /// </summary>
        Task<AreaDto> CreateAsync(AreaDto dto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Actualiza los datos de un área existente y devuelve el DTO actualizado.
        /// </summary>
        Task<AreaDto> UpdateAsync(Guid areaId, AreaDto dto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Activa un área.
        /// </summary>
        Task ActivateAsync(Guid areaId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Desactiva un área.
        /// </summary>
        Task DeactivateAsync(Guid areaId, CancellationToken cancellationToken = default);
    }
}