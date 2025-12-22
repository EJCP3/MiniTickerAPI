using System.Threading.Tasks;
using MiniTicker.Core.Application.DTOs.Auth;

namespace MiniTicker.Core.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    }
}