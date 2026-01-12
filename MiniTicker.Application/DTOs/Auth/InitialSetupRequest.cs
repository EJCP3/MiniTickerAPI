using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace MiniTicker.Core.Application.DTOs.Auth
{
    public class InitialSetupRequest
    {
        [Required]
        public string NewPassword { get; set; } = string.Empty;

        public IFormFile? FotoPerfil { get; set; }
    }
}