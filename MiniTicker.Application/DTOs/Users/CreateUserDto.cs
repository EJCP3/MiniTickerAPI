using System;
using Microsoft.AspNetCore.Http;
using MiniTicker.Core.Domain.Enums;

namespace MiniTicker.Core.Application.Users
{
    public class CreateUserDto
    {
        public string Nombre { get; set; } 
        public string Email { get; set; } 
        public string Password { get; set; }
        public Rol Rol { get; set; } = Rol.Solicitante;
        public Guid? AreaId { get; set; }
        public bool Activo { get; set; } = true;
        public IFormFile? FotoPerfil { get; set; }
    }

}