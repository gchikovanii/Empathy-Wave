using CloudinaryDotNet.Actions;
using EmphatyWave.Application.Services.Cloudinaries.Abstraction;
using EmphatyWave.Application.Services.PromoCodeImages.Abstraction;
using EmphatyWave.Application.Services.PromoCodeImages.DTOs;
using EmphatyWave.Domain;
using EmphatyWave.Persistence.Repositories.Abstraction;
using EmphatyWave.Persistence.UOW;

namespace EmphatyWave.Application.Services.PromoCodeImages.Implementation
{
    public class ProductImageService(ICloudinaryService cloudinaryService,IProductImageRepository productImageRepository, 
        IUnitOfWork unit
        ) : IProductImageService
    {
        private readonly IProductImageRepository _productImageRepository = productImageRepository;
        private readonly ICloudinaryService _cloudinaryService = cloudinaryService;
        private readonly IUnitOfWork _unit = unit;
        public async Task<bool> UploadImage(CancellationToken token,CreateProductImageDto input)
        {
            var result = await _cloudinaryService.UploadImage(input.File);
            await _productImageRepository.CreateProductImageAsync(token,new ProductImage()
            {
                ProductId = input.ProductId,
                PublicId = result.PublicId,
                Url = result.Url.AbsoluteUri
            });
            return await _unit.SaveChangesAsync(token).ConfigureAwait(false);
        }

        public async Task<bool> UplaodImages(CancellationToken token, List<CreateProductImageDto> input)
        {
            List<Task<ImageUploadResult>> imageUploadResults = new List<Task<ImageUploadResult>>();
            foreach (var item in input)
            {
                imageUploadResults.Add(_cloudinaryService.UploadImage(item.File));
            }
            var uploadImages = await Task.WhenAll(imageUploadResults);

            foreach (var image in uploadImages)
            {
                await _productImageRepository.CreateProductImageAsync(token,new ProductImage()
                {
                    ProductId = input.FirstOrDefault().ProductId,
                    PublicId = image.PublicId,
                    Url = image.Url.AbsoluteUri
                });
            }
            return true;
        }

    }
}
