namespace OrcaFacil.Application.DTOs.Auth;

public record AuthResponseDto(string Token, DateTime ExpiresAt, CurrentUserDto User);

public record CurrentUserDto(Guid Id, string Name, string Email, string Role, Guid CompanyId, string CompanyName);
