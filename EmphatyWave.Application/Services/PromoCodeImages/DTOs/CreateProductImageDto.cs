using Microsoft.AspNetCore.Http;

namespace EmphatyWave.Application.Services.PromoCodeImages.DTOs
{
    public class CreateProductImageDto
    {
        public IFormFile? File { get; set; }
        public Guid ProductId { get; set; }
    }
}
