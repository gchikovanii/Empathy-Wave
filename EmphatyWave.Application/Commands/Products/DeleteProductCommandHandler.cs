using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Common;
using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Products;
using EmphatyWave.Persistence.Repositories.Abstraction;
using EmphatyWave.Persistence.UOW;
using MediatR;
using System.Transactions;

namespace EmphatyWave.Application.Commands.Products
{
    public class DeleteProductCommandHandler(IProductRepository repo, IProductImageRepository imageRepo,
        IUnitOfWork unit) : IRequestHandler<DeleteProductCommand, Result>
    {
        private readonly IProductRepository _repo = repo;
        private readonly IUnitOfWork _unit = unit;
        private readonly IProductImageRepository _imageRepo = imageRepo;

        public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            using var transaction = await _unit.BeginTransaction(IsolationLevel.RepeatableRead, cancellationToken).ConfigureAwait(false);
            try
            {
                var productExists = await _repo.GetProductById(cancellationToken, request.Id).ConfigureAwait(false);
                if (productExists == null)
                    return Result.Failure(ProductErrors.ProductNotFound);
                _repo.DeleteProduct(productExists);

                var listOfImages = await _imageRepo.GetImages(cancellationToken, request.Id).ConfigureAwait(false);
                foreach (var image in listOfImages)
                {
                    await _imageRepo.DeleteProductImage(cancellationToken, image.Id).ConfigureAwait(false);
                }

                var saves = await _unit.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                if (saves == false)
                {
                    transaction.Rollback();
                    return Result.Failure(UnitError.CantSaveChanges);
                }
                transaction.Commit();
                return Result.Success();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return Result.Failure(new Error("Unexpected Exception", ex.Message));
            }
        }
    }
}
