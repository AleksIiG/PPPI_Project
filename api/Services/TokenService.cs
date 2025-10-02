using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using api.Models;
using api.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace api.Services
{
    public class TokenService : ITokenService
    {
        private readonly string _secretKey;
        private readonly string? _issuer;
        private readonly string? _audince;
        private readonly int _jwtExpires;
        public TokenService(IConfiguration config)
        {
            _secretKey = config["JWT_SECRET"] ?? throw new ArgumentNullException("JWT_SECRET is missing");
            _issuer = config["JWT_ISSUER"];
            _audince = config["JWT_AUDIENCE"];
            _jwtExpires = int.Parse(config["JWT_EXPIRES_MINUTES"] ?? "15");

        }

        public string CreateAccessToken(AppUser appuser)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, appuser.Id.ToString()),
                new Claim(ClaimTypes.Name, appuser.Username),
                new Claim(ClaimTypes.Email, appuser.Email),
                new Claim(ClaimTypes.Role, appuser.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audince,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtExpires),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public RefreshToken CreateRefreshToken(string ipAddress)
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                var token = Convert.ToBase64String(randomNumber);

                return new RefreshToken
                {
                    Token = token,
                    Expires = DateTime.UtcNow.AddDays(_jwtExpires),
                    Created = DateTime.UtcNow,
                    CreatedByIp = ipAddress,
                    IsRevoked = false
                };
            }
        }

        
    }
}