using OrcaFacil.Infrastructure.Authentication;
using Xunit;

namespace OrcaFacil.Tests.Authentication;

public class Pbkdf2PasswordHasherTests
{
    private readonly Pbkdf2PasswordHasher _hasher = new();

    [Fact]
    public void Verify_ComSenhaCorreta_DeveRetornarTrue()
    {
        var hash = _hasher.Hash("SenhaForte123!");

        Assert.True(_hasher.Verify("SenhaForte123!", hash));
    }

    [Fact]
    public void Verify_ComSenhaErrada_DeveRetornarFalse()
    {
        var hash = _hasher.Hash("SenhaForte123!");

        Assert.False(_hasher.Verify("SenhaErrada", hash));
    }

    [Fact]
    public void Hash_MesmaSenhaDuasVezes_DeveGerarHashesDiferentes()
    {
        // O salt é aleatório a cada chamada — dois hashes da mesma senha
        // nunca devem ser iguais. Isso é o que impede um atacante de perceber
        // que dois usuários usam a mesma senha só olhando a coluna no banco.
        var hash1 = _hasher.Hash("SenhaForte123!");
        var hash2 = _hasher.Hash("SenhaForte123!");

        Assert.NotEqual(hash1, hash2);
        Assert.True(_hasher.Verify("SenhaForte123!", hash1));
        Assert.True(_hasher.Verify("SenhaForte123!", hash2));
    }
}
