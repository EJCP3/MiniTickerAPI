using Microsoft.AspNetCore.Http;
using MiniTicker.Core.Application.Interfaces.Services;

namespace MiniTicker.Infrastructure.Persistence.Services
{
    public class FileStorageService : IFileStorageService
    {
        public async Task<string> UploadAsync(IFormFile file, string folder)
        {
            // implementación fake por ahora
            await Task.CompletedTask;
            return $"uploads/{folder}/{Guid.NewGuid()}_{file.FileName}";
        }

        public async Task DeleteAsync(string fileUrl)
        {
            await Task.CompletedTask;
        }
    }
}
