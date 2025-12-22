using Microsoft.AspNetCore.Http;

namespace MiniTicker.Core.Application.Users
{
    public class UpdateUserProfileDto
    {
        public string Nombre { get; set; } = null!;
        public IFormFile? FotoPerfil { get; set; }
    }
}