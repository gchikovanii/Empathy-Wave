using EmphatyWave.Application.Commands.Categories;
using EmphatyWave.Application.Commands.Orders;
using EmphatyWave.Application.Commands.Products;
using EmphatyWave.Application.Validators.ProductValidators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmphatyWave.Application.Extensions
{
    public static class FluentValidationxtension
    {
        public static IServiceCollection AddFluentValidation(this IServiceCollection services) 
        {
            services.AddScoped<IValidator<CreateProductCommand>, CreateProductValidator>();
            services.AddScoped<IValidator<UpdateProductCommand>, UpdateProductValidator>();
            services.AddScoped<IValidator<MakeOrderCommand>, MakeOrderValidator>();
            return services;
        }
    }
}
