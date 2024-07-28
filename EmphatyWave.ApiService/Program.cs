using EmphatyWave.ApiService.Infrastructure.Extensions;
using EmphatyWave.ApiService.Infrastructure.HealtChecks;
using EmphatyWave.ApiService.Infrastructure.Request;
using EmphatyWave.Application.Behaviours;
using EmphatyWave.Application.Commands.Categories;
using EmphatyWave.Application.Commands.Orders;
using EmphatyWave.Application.Commands.Products;
using EmphatyWave.Application.Extensions;
using EmphatyWave.Application.Services.Cloudinaries.Models;
using EmphatyWave.Persistence.DataSeeding;
using EmphatyWave.Persistence.Infrastructure.GlobalException;
using HealthChecks.UI.Client;
using MediatR;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.AddRedisOutputCache("cache");
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks()
    .AddCheck<TimeIntervalOfProcessingOrderHelthCheck>("Order Processing Time", tags: new[] { "service" })
    .AddCheck<TimeIntervalOfFetchingCategoriesHelthCheck>("Category Fetching Speed", tags: new[] { "service" })
    .AddSqlServer(builder.Configuration.GetConnectionString("SqlConnectionString"));
builder.Services.Configure<GoogleAppSettings>(builder.Configuration.GetSection("GoogleAppSettings"));
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
builder.Services.Configure<CloudinarySetting>(builder.Configuration.GetSection("CloudinarySettings"));

#region MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(CreateProductCommandHandler).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(CreateCategoryCommandHandler).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(MakeOrderCommandHandler).Assembly);

});
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingPiplineBehaviour<,>));
#endregion
builder.Host.UseSerilog((context, config) =>
 {
     config
         .ReadFrom.Configuration(context.Configuration)
         .Enrich.FromLogContext()
         .WriteTo.Console()
         .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day);
 });

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
    endpoints.MapHealthChecks("/Healthes", new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });
    endpoints.MapHealthChecks("/Healthes/Services", new HealthCheckOptions
    {
        Predicate = reg => reg.Tags.Contains("service"),
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });
});
app.UseSerilogRequestLogging();
app.MapDefaultEndpoints();

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Fatal Exception, host is not responding!");
    throw;
}
finally
{
    Log.CloseAndFlush(); 
}

