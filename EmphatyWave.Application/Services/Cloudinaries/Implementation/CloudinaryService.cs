using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using EmphatyWave.Application.Services.Cloudinaries.Abstraction;
using EmphatyWave.Application.Services.Cloudinaries.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace EmphatyWave.Application.Services.Cloudinaries.Implementation
{
    public class CloudinaryService : ICloudinaryService
    {
        public readonly Cloudinary _cloudinary;
        public CloudinaryService(IOptions<CloudinarySetting> cloudinarySetting)
        {
            var setting = cloudinarySetting.Value;
            var account = new CloudinaryDotNet.Account() { Cloud = setting.Cloud, ApiKey = setting.ApiKey, ApiSecret = setting.ApiSecret };
            _cloudinary = new Cloudinary(account);
        }
        public async Task<ImageUploadResult> UploadImage(IFormFile file)
        {
            var result = new ImageUploadResult();
            if (file.Length > 0)
            {
                using var stream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams() { File = new FileDescription(file.FileName, stream) };
                result = await _cloudinary.UploadAsync(uploadParams);
            }
            return result;
        }
        public async Task<DeletionResult> DeleteImage(string publicId)
        {
            return await _cloudinary.DestroyAsync(new DeletionParams(publicId));
        }
    }
}
