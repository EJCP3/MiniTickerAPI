using System;
using System.Threading;
using System.Threading.Tasks;
using MiniTicker.Core.Application.Read;
using MiniTicker.Core.Application.Tickets;
using MiniTicker.Core.Application.Filters;
using MiniTicker.Core.Application.Shared;

namespace MiniTicker.Core.Application.Interfaces.Services
{
    /// <summary>
    /// Contrato de la capa Application que encapsula la lógica de negocio de tickets.
    /// Solo firmas asíncronas; no devuelve entidades del dominio.
    /// </summary>
    public interface ITicketService
    {
        /// <summary>
        /// Crea un nuevo ticket y devuelve su representación DTO.
        /// </summary>
        Task<TicketDto> CreateAsync(CreateTicketDto dto, Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Actualiza datos de un ticket existente y devuelve su representación DTO.
        /// </summary>
        Task<TicketDto> UpdateAsync(Guid ticketId, UpdateTicketDto dto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cambia el estado de un ticket. <paramref name="userId"/> es el actor que realiza el cambio.
        /// Devuelve el ticket actualizado en forma de DTO.
        /// </summary>
        Task<TicketDto> ChangeStatusAsync(Guid ticketId, ChangeTicketStatusDto dto, Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asigna un gestor al ticket.
        /// </summary>
        Task AssignManagerAsync(Guid ticketId, AssignTicketDto dto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene un listado paginado de tickets aplicando filtros.
        /// </summary>
        Task<PagedResultDto<TicketDto>> GetPagedAsync(TicketFilterDto filter, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene el detalle completo de un ticket por su Id.
        /// </summary>
        Task<TicketDetailDto?> GetByIdAsync(Guid ticketId, CancellationToken cancellationToken = default);
    }
}