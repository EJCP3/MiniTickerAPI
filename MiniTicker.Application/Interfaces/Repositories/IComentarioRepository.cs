using MiniTicker.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MiniTicker.Core.Application.Interfaces.Repositories
{
    /// <summary>
    /// Contrato para operaciones asíncronas sobre la entidad <see cref="Comentario"/>.
    /// Solo definición; sin implementación ni dependencias a EF Core.
    /// </summary>
    public interface IComentarioRepository
    {
       
        Task<Comentario> AddAsync(Comentario comentario, CancellationToken cancellationToken = default);

    
        Task<IReadOnlyList<Comentario>> GetByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Comentario>> GetByTicketIdOrderedByFechaAscAsync(Guid ticketId, CancellationToken cancellationToken = default);
    }
}