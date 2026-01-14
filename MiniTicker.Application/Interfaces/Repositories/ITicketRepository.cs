using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MiniTicker.Core.Domain.Entities;
using MiniTicker.Core.Application.Filters;
using System.Linq.Expressions;
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

        Task<bool> AnyAsync(Expression<Func<Ticket, bool>> predicate, CancellationToken cancellationToken = default);
        Task<Dictionary<int, int>> GetStatusSummaryAsync(TicketFilterDto filter);

        IQueryable<Ticket> GetAllAsQueryable();
    }
}