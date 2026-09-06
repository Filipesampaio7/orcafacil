using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OrcaFacil.Application.Interfaces;
using OrcaFacil.Domain.Entities;

namespace OrcaFacil.Infrastructure.Authentication;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public AuthTokenResult GenerateToken(User user)
    {
        var jwtSection = _configuration.GetSection("Jwt");
        var key = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException(
                "Jwt:Key não configurado. Defina via variável de ambiente Jwt__Key ou dotnet user-secrets.");

        var expirationMinutes = jwtSection.GetValue<int?>("ExpirationMinutes") ?? 60;
        var expiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);

        // "companyId"/"companyName" são claims customizadas (não fazem parte
        // do padrão JWT) — é assim que multi-tenant normalmente viaja dentro
        // do token: toda requisição autenticada já chega sabendo de qual
        // empresa é, sem precisar consultar o banco de novo a cada request.
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("companyId", user.CompanyId.ToString()),
            new Claim("companyName", user.Company.Name),
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtSection["Issuer"],
            audience: jwtSection["Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return new AuthTokenResult(tokenString, expiresAt);
    }
}
