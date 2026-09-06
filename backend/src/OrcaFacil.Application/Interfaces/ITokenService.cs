using OrcaFacil.Domain.Entities;

namespace OrcaFacil.Application.Interfaces;

public interface ITokenService
{
    AuthTokenResult GenerateToken(User user);
}

public record AuthTokenResult(string Token, DateTime ExpiresAt);
