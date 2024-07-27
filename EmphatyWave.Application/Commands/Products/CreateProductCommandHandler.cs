using EmphatyWave.Application.Services.PromoCodeImages.Abstraction;
using EmphatyWave.Application.Services.PromoCodeImages.DTOs;
using EmphatyWave.Domain;
using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Categories;
using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Common;
using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Products;
using EmphatyWave.Persistence.Repositories.Abstraction;
using EmphatyWave.Persistence.UOW;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EmphatyWave.Application.Commands.Products
{
    public class CreateProductCommandHandler(IProductRepository repository, ICategoryRepository categoryRepo,
        IValidator<CreateProductCommand> validator,IUnitOfWork unit, IProductImageService productImageService, ILogger<CreateProductCommandHandler> logger
        ) : IRequestHandler<CreateProductCommand, Result>
    {
        private readonly IProductImageService _productImageService = productImageService;
        private readonly ICategoryRepository _categoryRepo = categoryRepo;
        private readonly IProductRepository _repository = repository;
        private readonly ILogger<CreateProductCommandHandler> _logger = logger;
        private readonly IUnitOfWork _unit = unit;
        private readonly IValidator<CreateProductCommand> _validator = validator;

        public async Task<Result> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            using var transaction = await _unit.BeginTransaction(System.Transactions.IsolationLevel.RepeatableRead, cancellationToken).ConfigureAwait(false);

            try
            {
                ValidationResult result = await _validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
                if (!result.IsValid)
                {
                    var errorMessages = result.Errors.Select(e => e.ErrorMessage);
                    string errorMessage = string.Join("; ", errorMessages);
                    Error error = new("ValidationError", errorMessage);
                    return Result.Failure(error);
                }
                var category = await _categoryRepo.GetCategoryById(cancellationToken, request.CategoryId).ConfigureAwait(false);
                if (category == null)
                    return Result.Failure(CategoryErrors.CategoryNotExists);
                var prodExists = await _repository.GetProductByName(cancellationToken, request.Name).ConfigureAwait(false);
                if (prodExists == null && prodExists.Name == null)
                    return Result.Failure(ProductErrors.AlreadyExists);
                var product = new Product
                {
                    Price = request.Price,
                    SKU = request.SKU,
                    CategoryId= request.CategoryId,
                    Description = request.Description,
                    Discount = request.Discount,
                    Name = request.Name,
                    Quantity = request.Quantity,
                    Title = request.Title,
                    Id = Guid.NewGuid()
                };
                
                var productId = product.Id;
                // Upload images
                var imageResult = await _productImageService.UplaodImages(cancellationToken, request.GetImageFiles()
                               .Select(i => new CreateProductImageDto()
                               {
                                   File = i,
                                   ProductId = productId
                               }).ToList()).ConfigureAwait(false);

                await _repository.CreateProductAsync(cancellationToken, product).ConfigureAwait(false);
                if (!imageResult)
                {
                    transaction.Rollback();
                    return Result.Failure(new Error("Image.UploadError", "Image hasn't uploaded, but product added successfully"));
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
                _logger.LogError(ex, "Error while creating product {Error}", ex.Message);
                return Result.Failure(new Error("Unexpected Exception", ex.Message));
            }
        }
    }
}
