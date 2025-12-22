using MiniTicker.Core.Domain.Entities;

namespace MiniTicker.Core.Application.Interfaces.Services
{
    public interface IJwtTokenService
    {
        string GenerateToken(Usuario usuario);
    }
}
