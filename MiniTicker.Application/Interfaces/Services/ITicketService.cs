using MiniTicker.Core.Application.Comments;
using MiniTicker.Core.Application.Filters;
using MiniTicker.Core.Application.Read;
using MiniTicker.Core.Application.Shared;
using MiniTicker.Core.Application.Tickets;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MiniTicker.Core.Application.Interfaces.Services
{
    /// <summary>
    /// Contrato de la capa Application que encapsula la lógica de negocio de tickets.
    /// Solo firmas asíncronas; no devuelve entidades del dominio.
    /// </summary>
    public interface ITicketService
    {
      
        Task<TicketDto> CreateAsync(CreateTicketDto dto, Guid userId, CancellationToken cancellationToken = default);

        Task<TicketDto> UpdateAsync(Guid ticketId, UpdateTicketDto dto, CancellationToken cancellationToken = default);

        
        Task<TicketDto> ChangeStatusAsync(Guid ticketId, ChangeTicketStatusDto dto, Guid userId, CancellationToken cancellationToken = default);

    
        Task AssignManagerAsync(Guid ticketId, AssignTicketDto dto, CancellationToken cancellationToken = default);

       
        Task<PagedResultDto<TicketDto>> GetPagedAsync(TicketFilterDto filter, CancellationToken cancellationToken = default);

       
        Task<TicketDetailDto?> GetByIdAsync(Guid ticketId, CancellationToken cancellationToken = default);

        Task<ComentarioDto> AddCommentAsync(  Guid ticketId,  CreateComentarioDto dto,  Guid userId,  CancellationToken cancellationToken = default);
    }
}
