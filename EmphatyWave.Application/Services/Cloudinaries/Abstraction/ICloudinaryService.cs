using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;

namespace EmphatyWave.Application.Services.Cloudinaries.Abstraction
{
    public interface ICloudinaryService
    {
        Task<ImageUploadResult> UploadImage(IFormFile file);
        Task<DeletionResult> DeleteImage(string publicId);
    }
}
