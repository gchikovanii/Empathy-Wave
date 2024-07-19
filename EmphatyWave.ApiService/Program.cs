using EmphatyWave.ApiService;
using EmphatyWave.ApiService.Infrastructure.Extensions;
using EmphatyWave.Application.Commands.Categories;
using EmphatyWave.Application.Commands.Orders;
using EmphatyWave.Application.Commands.Products;
using EmphatyWave.Application.Extensions;
using EmphatyWave.Persistence.DataSeeding;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.AddRedisOutputCache("cache");

builder.Services.AddControllers();
builder.AddServiceDefaults();
builder.Services.AddAuthentication().AddBearerToken(IdentityConstants.BearerScheme);
builder.Services.AddAuthorization();
builder.Services.AddServiceExtension(builder.Configuration);
builder.Services.AddSwaggerConfiguration();
builder.Services.AddIdentityConfiguration(builder.Configuration);
builder.Services.AddJwtConfiguration(builder.Configuration);
builder.Services.AddStripeSettings(builder.Configuration);
builder.Services.AddFluentValidation();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
#region MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(CreateProductCommandHandler).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(CreateCategoryCommandHandler).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(MakeOrderCommandHandler).Assembly);

});

#endregion


var app = builder.Build();
#region SeedData
SeedData.Initialize(app.Services, new SuperAdminDto { Email = builder.Configuration["SuperAdmin:Email"],
    UserName = builder.Configuration["SuperAdmin:UserName"], Password = builder.Configuration["SuperAdmin:Password"]});
#endregion
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCulture();
app.UseExceptionHandler();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseOutputCache();

//Mapping controllers(pointing to controllers that are created in project)
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});
app.MapDefaultEndpoints();
app.Run();

