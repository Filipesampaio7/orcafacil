namespace OrcaFacil.Application.DTOs.Auth;

/// <summary>
/// Cadastro cria uma Company nova e o primeiro User (Owner) dela numa única
/// operação — não existe "criar usuário sem empresa" no OrçaFácil.
/// </summary>
public record RegisterRequestDto(string CompanyName, string UserName, string Email, string Password);
