using System.Security.Cryptography;
using OrcaFacil.Application.Interfaces;

namespace OrcaFacil.Infrastructure.Authentication;

/// <summary>
/// PBKDF2-HMAC-SHA256 com salt aleatório por senha e 100.000 iterações.
/// Formato armazenado: "{iterações}.{salt em base64}.{hash em base64}" — guardar
/// o número de iterações junto permite aumentar a segurança no futuro (subir
/// as iterações) sem invalidar hashes antigos já salvos.
/// </summary>
public class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int SaltSizeBytes = 16;
    private const int KeySizeBytes = 32;
    private const int Iterations = 100_000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSizeBytes);
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySizeBytes);

        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
    }

    public bool Verify(string password, string passwordHash)
    {
        var parts = passwordHash.Split('.', 3);
        if (parts.Length != 3 || !int.TryParse(parts[0], out var iterations))
        {
            return false;
        }

        var salt = Convert.FromBase64String(parts[1]);
        var expectedKey = Convert.FromBase64String(parts[2]);

        var actualKey = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, Algorithm, expectedKey.Length);

        // Comparação em tempo constante: evita que a duração da checagem
        // vaze informação sobre até onde o hash "bateu" (timing attack).
        return CryptographicOperations.FixedTimeEquals(actualKey, expectedKey);
    }
}
