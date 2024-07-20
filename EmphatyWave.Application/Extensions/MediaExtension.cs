using Microsoft.AspNetCore.Http;

namespace EmphatyWave.Application.Extensions
{
    public static class MediaExtension
    {
        public static IFormFile ConvertBase64ToImage(this string equipmentFiles)
        {
            byte[] bytes = Convert.FromBase64String(equipmentFiles);
            MemoryStream stream = new MemoryStream(bytes);

            IFormFile file = new FormFile(stream, 0, bytes.Length, Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

            return file;
        }
    }
}
