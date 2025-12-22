using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MiniTicker.Core.Domain.Entities;
using MiniTicker.Core.Application.Filters;

namespace MiniTicker.Core.Application.Interfaces.Repositories
{
    public interface ITicketRepository
    {
        Task AddAsync(Ticket ticket);
        Task UpdateAsync(Ticket ticket);
        Task DeleteAsync(Ticket ticket);
        Task<Ticket?> GetByIdAsync(Guid ticketId);
        Task<Ticket?> GetByNumeroAsync(string numero);
        Task<bool> ExistsAsync(Guid ticketId);
        Task<IReadOnlyList<Ticket>> GetAllAsync();
        Task<(IReadOnlyList<Ticket> Tickets, int TotalCount)> GetPagedAsync(TicketFilterDto filter);
        Task<int> GetNextSequenceForYearAsync(int year);

    }
}