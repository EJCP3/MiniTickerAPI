using MiniTicker.Core.Application.Catalogs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MiniTicker.Core.Application.Interfaces.Services
{
    public interface IAreaService
    {
        // ARREGLO CS0121: Un solo método con el parámetro opcional 'incluirInactivos'
        Task<IReadOnlyList<AreaDto>> GetAllAsync(bool incluirInactivos = false, CancellationToken cancellationToken = default);

        Task<AreaDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<AreaDto> CreateAsync(AreaDto dto, CancellationToken cancellationToken = default);
        Task<AreaDto> UpdateAsync(Guid id, AreaDto dto, CancellationToken cancellationToken = default);

        Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
        Task ActivateAsync(Guid id, CancellationToken cancellationToken = default);

        // ARREGLO CS1061: Agregamos el método que faltaba
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}