using EmphatyWave.Domain;
using EmphatyWave.Persistence.Infrastructure.ErrorsAggregate.Common;
using EmphatyWave.Persistence.Repositories.Abstraction;
using EmphatyWave.Persistence.UOW;
using FluentValidation;
using FluentValidation.Results;
using Mapster;
using MediatR;

namespace EmphatyWave.Application.Commands.Categories
{
    public class CreateCategoryCommandHandler(ICategoryRepository repo, IUnitOfWork unit, IValidator<CreateCategoryCommand> validator
        ) : IRequestHandler<CreateCategoryCommand, Result>
    {
        private readonly ICategoryRepository _repo = repo;
        private readonly IValidator<CreateCategoryCommand> _validator = validator;
        private readonly IUnitOfWork _unit = unit;
        public async Task<Result> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            ValidationResult result = await _validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);

            if (!result.IsValid)
            {
                var errorMessages = result.Errors.Select(e => e.ErrorMessage);
                string errorMessage = string.Join("; ", errorMessages);
                Error error = new("ValidationError", errorMessage);
                return Result.Failure(error);
            }
            await _repo.CreateCategoryAsync(cancellationToken, request.Adapt<Category>()).ConfigureAwait(false);
            var res = await _unit.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            if (res == false)
                return Result.Failure(UnitError.CantSaveChanges);
            return Result.Success();
        }
    }
}
