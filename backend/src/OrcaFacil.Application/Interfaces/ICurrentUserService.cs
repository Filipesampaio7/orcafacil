namespace OrcaFacil.Application.Interfaces;

/// <summary>
/// Abstrai "quem está fazendo esta requisição" a partir do token JWT validado.
/// A partir da FASE 4, todo Service que precisa filtrar dados por empresa
/// (ex.: "só listar clientes da minha Company") vai depender disto em vez de
/// receber o CompanyId como parâmetro solto — evita que alguém esqueça de
/// filtrar e vaze dado de uma empresa para outra.
/// </summary>
public interface ICurrentUserService
{
    bool IsAuthenticated { get; }
    Guid? UserId { get; }
    Guid? CompanyId { get; }
    string? Email { get; }
    string? Name { get; }
    string? CompanyName { get; }
}
