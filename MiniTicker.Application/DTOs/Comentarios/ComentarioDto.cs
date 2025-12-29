using MiniTicker.Core.Application.Users;

namespace MiniTicker.Core.Application.Comments
{
    public class ComentarioDto
    {
        public UserDto Usuario { get; set; } = null!;
        public string Texto { get; set; } = null!;
        public string Fecha { get; set; } = null!;
    }
}