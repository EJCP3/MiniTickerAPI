namespace MiniTicker.Core.Application.Auth
{
    public class LoginResponseDto
    {
        public string Token { get; set; } = null!;
        public Guid UsuarioId { get; set; }
        public string Rol { get; set; } = null!;
    }
}