using MiniTicker.Core.Application.Actividad;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniTicker.Core.Application.Interfaces.Services
{
    public interface IActivityService
    {
        Task<IReadOnlyList<ActivityDto>> GetMyActivityAsync(Guid userId);
        // Le pasamos el userId actual solo para saber si poner "Tú" o el nombre del usuario
        Task<IReadOnlyList<ActivityDto>> GetGlobalActivityAsync(Guid currentUserId, Guid? areaId = null, Guid? targetUserId = null);
    }
}