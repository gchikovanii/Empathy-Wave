using EmphatyWave.Application.Extensions;
using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Common;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace EmphatyWave.Application.Commands.Products
{
    public class CreateProductCommand : IRequest<Result>
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal? Discount { get; set; }
        public int Quantity { get; set; }
        public string SKU { get; set; }
        public Guid CategoryId { get; set; }
        public List<string>? Images { get; set; }

        public List<IFormFile>? GetImageFiles()
        {
            try
            {
                var imageFiles = new List<IFormFile>();
                if (Images.Count == 0 || Images == null)
                    return default;
                foreach (var image in Images)
                {
                    imageFiles.Add(image.ConvertBase64ToImage());
                }
                return imageFiles;
            }
            catch (Exception)
            {
                return default;
            }

        }
    }
}
