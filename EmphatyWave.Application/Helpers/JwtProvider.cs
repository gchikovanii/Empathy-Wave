using EmphatyWave.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EmphatyWave.Application.Helpers
{
    public class JwtProvider
    {
        private readonly IConfiguration _config;
        private readonly SymmetricSecurityKey _key;
        public JwtProvider(IConfiguration config)
        {
            _config = config;
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Token:Key"]));
        }

        public string CreateToken(User user, string role)
        {
            try
            {
                var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Email, user.Email)
            };
                claims.Add(new Claim(ClaimTypes.Role, role));
                var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddHours(12),
                    SigningCredentials = creds,
                    Issuer = _config["Token:Issuer"]
                };
                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                //Handle more efficient!
                return String.Empty;
            }
        }
    }
}
