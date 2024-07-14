using EmphatyWave.Application.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace EmphatyWave.Application.Extensions
{
    public static class JWTExtension
    {
        public static IServiceCollection AddJwtConfiguration(this IServiceCollection services, IConfiguration config)
        {
            services.AddScoped<JwtProvider>();
            services.AddAuthentication(opts => {
                opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                opts.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
              .AddJwtBearer(options =>
              {

                  options.TokenValidationParameters = new TokenValidationParameters
                  {
                      ValidateIssuerSigningKey = true,
                      IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Token:Key"])),
                      ValidIssuer = config["Token:Issuer"],
                      ValidateIssuer = true,
                      ValidateAudience = false
                  };
                  options.Events = new JwtBearerEvents
                  {
                      OnMessageReceived = context =>
                      {
                          context.Token = context.Request.Cookies[config["Token:CookieName"]];
                          return Task.CompletedTask;
                      }
                  };
              });
            services.AddAuthorization();
            return services;
        }
    }
}
