using EmphatyWave.Application.Services.PromoCodeImages.DTOs;

namespace EmphatyWave.Application.Services.PromoCodeImages.Abstraction
{
    public interface IProductImageService
    {
        Task<bool> UploadImage(CancellationToken token, CreateProductImageDto input);
        Task<bool> UplaodImages(CancellationToken token, List<CreateProductImageDto> input);
    }
}
