using MiniTicker.Core.Application.Users;

namespace MiniTicker.Core.Application.Auth
{
    public class LoginResponseDto
    {
        public string Token { get; set; } = null!;

        public UserDto User { get; set; } = null!;

        public bool DebeCambiarPassword { get; set; }
    }
}