using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TennisReservation.Application.Interfaces;
using TennisReservation.Contracts.Users.Dto;

namespace TennisReservation.Infrastructure.Postgres.Services
{
    public class JwtProvider : IJwtProvider
    {
        private readonly JwtOptions _options;

        public JwtProvider(IOptions<JwtOptions> options) => _options = options.Value;
        public string GenerateToken(UserLoginDto user)
        {
            Claim[] claims = [
                new("userId", user.UserId.ToString()),
                new(ClaimTypes.Role, user.Role.ToString()),
                new Claim(ClaimTypes.Name, user.Email)];

            var signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey)), SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                claims: claims,
                signingCredentials: signingCredentials,
                expires: DateTime.UtcNow.AddHours(_options.ExpiresHours)
                );

            var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

            return tokenValue;
        }
    }
}
