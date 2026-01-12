using System.Threading.Tasks;
using MiniTicker.Core.Application.Auth;
using MiniTicker.Core.Application.DTOs.Auth;

namespace MiniTicker.Core.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(LoginRequestDto request);

        Task LogoutAsync();

        Task<string> CompleteSetupAsync(Guid userId, InitialSetupRequest request);
    }
}