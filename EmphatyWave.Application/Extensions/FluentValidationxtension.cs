using EmphatyWave.Application.Commands.Categories;
using EmphatyWave.Application.Commands.Orders;
using EmphatyWave.Application.Commands.Products;
using EmphatyWave.Application.Services.Account.DTOs;
using EmphatyWave.Application.Services.PromoCodes.DTOs;
using EmphatyWave.Application.Validators.Categories;
using EmphatyWave.Application.Validators.ProductValidators;
using EmphatyWave.Application.Validators.PromoCodeValidator;
using EmphatyWave.Application.Validators.UserValidators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;


namespace EmphatyWave.Application.Extensions
{
    public static class FluentValidationxtension
    {
        public static IServiceCollection AddFluentValidation(this IServiceCollection services) 
        {
            services.AddScoped<IValidator<PromoCodeDto>, PromoCodeDtoValidator>();
            services.AddScoped<IValidator<CreateProductCommand>, CreateProductValidator>();
            services.AddScoped<IValidator<UpdateProductCommand>, UpdateProductValidator>();
            services.AddScoped<IValidator<MakeOrderCommand>, MakeOrderValidator>();
            services.AddScoped<IValidator<CreateCategoryCommand>, CategoryValidator>();
            services.AddScoped<IValidator<RegisterDto>, RegisterDtoValidator>();
            return services;
        }
    }
}
