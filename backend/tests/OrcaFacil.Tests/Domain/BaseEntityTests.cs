using OrcaFacil.Domain.Common;
using Xunit;

namespace OrcaFacil.Tests.Domain;

// Como BaseEntity é abstrata, criamos uma classe mínima só para o teste.
file class DummyEntity : BaseEntity
{
}

public class BaseEntityTests
{
    [Fact]
    public void NovaEntidade_DeveGerarIdUnico()
    {
        var entity1 = new DummyEntity();
        var entity2 = new DummyEntity();

        Assert.NotEqual(Guid.Empty, entity1.Id);
        Assert.NotEqual(entity1.Id, entity2.Id);
    }

    [Fact]
    public void NovaEntidade_DeveDefinirCreatedAtAutomaticamente()
    {
        var before = DateTime.UtcNow;
        var entity = new DummyEntity();
        var after = DateTime.UtcNow;

        Assert.InRange(entity.CreatedAt, before, after);
        Assert.Null(entity.UpdatedAt);
    }
}
