using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MiniTicker.Core.Application.Interfaces.Services
{
    public interface IFileStorageService
    {
        /// <summary>
        /// Sube el fichero al almacenamiento y devuelve la URL pública o identificador.
        /// </summary>
        Task<string> UploadAsync(IFormFile file, string folder);

        /// <summary>
        /// Elimina un archivo por su URL/identificador.
        /// </summary>
        Task DeleteAsync(string fileUrl);
    }
} 