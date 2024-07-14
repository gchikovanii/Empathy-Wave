using EmphatyWave.Application.Extensions;
using EmphatyWave.Persistence.DataSeeding;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddControllers();
builder.AddServiceDefaults();
builder.Services.AddAuthentication().AddBearerToken(IdentityConstants.BearerScheme);
builder.Services.AddAuthorization();

builder.Services.AddServiceExtension(builder.Configuration);
builder.Services.AddSwaggerConfiguration();
builder.Services.AddIdentityConfiguration(builder.Configuration);
builder.Services.AddJwtConfiguration(builder.Configuration);
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();



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

app.UseExceptionHandler();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
//Mapping controllers(pointing to controllers that are created in project)
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.MapDefaultEndpoints();
app.Run();

