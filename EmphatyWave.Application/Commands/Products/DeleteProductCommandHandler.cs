using EmphatyWave.Persistence.Repositories.Abstraction;
using EmphatyWave.Persistence.Repositories.Implementation;
using EmphatyWave.Persistence.UOW;
using MediatR;

namespace EmphatyWave.Application.Commands.Products
{
    public class DeleteProductCommandHandler(IProductRepository repo, IProductImageRepository imageRepo,
        IUnitOfWork unit) : IRequestHandler<DeleteProductCommand, bool>
    {
        private readonly IProductRepository _repo = repo;
        private readonly IUnitOfWork _unit = unit;
        private readonly IProductImageRepository _imageRepo = imageRepo;

        public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            await _repo.DeleteProduct(cancellationToken, request.Id).ConfigureAwait(false);
            var result = await _unit.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            if (result == true)
            {
                var listOfImages = await _imageRepo.GetImages(cancellationToken, request.Id).ConfigureAwait(false);
                foreach (var image in listOfImages)
                {
                    await _imageRepo.DeleteProductImage(cancellationToken, image.Id).ConfigureAwait(false);
                }
            }
            return result;
        }
    }
}
