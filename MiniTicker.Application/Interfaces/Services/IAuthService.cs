using MiniTicker.Core.Application.Auth;
using System.Threading.Tasks;

namespace MiniTicker.Core.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    }
}