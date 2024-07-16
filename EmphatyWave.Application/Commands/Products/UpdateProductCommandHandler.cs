﻿using EmphatyWave.Application.Validators.ProductValidators;
using EmphatyWave.Domain;
using EmphatyWave.Persistence.Repositories.Implementation;
using EmphatyWave.Persistence.UOW;
using FluentValidation;
using FluentValidation.Results;
using Mapster;
using MediatR;

namespace EmphatyWave.Application.Commands.Products
{
    public class UpdateProductCommandHandler(ProductRepository repo, IValidator<UpdateProductCommand> validator,
        IUnitOfWork unit) : IRequestHandler<UpdateProductCommand, bool>
    {
        private readonly ProductRepository _repo = repo;
        private readonly IUnitOfWork _unitOfWork = unit;
        private readonly IValidator<UpdateProductCommand> _validator = validator; 
        public async Task<bool> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            ValidationResult result = await _validator.ValidateAsync(request,cancellationToken).ConfigureAwait(false);
            if (!result.IsValid)
                throw new ValidationException(result.Errors);
            await _repo.UpdateProduct(cancellationToken,request.Adapt<Product>()).ConfigureAwait(false); 
            return await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
