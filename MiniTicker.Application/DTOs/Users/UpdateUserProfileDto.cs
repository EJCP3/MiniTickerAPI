using Microsoft.AspNetCore.Http;
using MiniTicker.Core.Domain.Enums;
using System;

namespace MiniTicker.Core.Application.Users
{
    public class UpdateUserDto 
    {
        public string? Nombre { get; set; } 
        public string? Email { get; set; } 

        public string? Password { get; set; }

        public Rol Rol { get; set; }
        public Guid? AreaId { get; set; }
        public bool Activo { get; set; }

        public IFormFile? FotoPerfil { get; set; }
    }
}