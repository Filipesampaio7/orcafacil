namespace OrcaFacil.Application.Interfaces;

/// <summary>
/// A Application só conhece este contrato — não sabe (nem precisa saber) que
/// por baixo é PBKDF2. Isso deixa o algoritmo trocável sem tocar em AuthService.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string passwordHash);
}
