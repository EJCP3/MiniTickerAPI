using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MiniTicker.Core.Domain.Entities;

namespace MiniTicker.Core.Application.Interfaces.Repositories
{
    public interface ITicketEventRepository
    {
        Task AddAsync(TicketEvent evt, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<TicketEvent>> GetByTicketIdOrderedAscAsync(Guid ticketId, CancellationToken cancellationToken = default);
    }
}