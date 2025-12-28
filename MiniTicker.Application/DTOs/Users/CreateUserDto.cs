using System;
using Microsoft.AspNetCore.Http;
using MiniTicker.Core.Domain.Enums;

namespace MiniTicker.Core.Application.Users
{
    public class CreateUserDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty; 
        public Rol Rol { get; set; } = Rol.Solicitante;
        public Guid? AreaId { get; set; }
        public bool Activo { get; set; } = true;
        public IFormFile? FotoPerfil { get; set; }
    }

}