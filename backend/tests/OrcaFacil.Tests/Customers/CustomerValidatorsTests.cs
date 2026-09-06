using OrcaFacil.Application.DTOs.Customers;
using OrcaFacil.Application.Validators.Customers;
using Xunit;

namespace OrcaFacil.Tests.Customers;

public class CreateCustomerValidatorTests
{
    private readonly CreateCustomerValidator _validator = new();

    [Fact]
    public void Validate_ComApenasNomeObrigatorio_DeveSerValido()
    {
        var request = new CreateCustomerDto("João da Silva", null, null, null, null, null);

        Assert.True(_validator.Validate(request).IsValid);
    }

    [Fact]
    public void Validate_SemNome_DeveGerarErro()
    {
        var request = new CreateCustomerDto("", null, null, null, null, null);

        Assert.False(_validator.Validate(request).IsValid);
    }

    [Fact]
    public void Validate_ComEmailInvalido_DeveGerarErro()
    {
        var request = new CreateCustomerDto("João da Silva", null, "nao-e-email", null, null, null);

        Assert.False(_validator.Validate(request).IsValid);
    }

    [Fact]
    public void Validate_ComEmailVazio_NaoDeveGerarErro()
    {
        // E-mail é opcional — string vazia/nula não deve disparar a regra de
        // formato (só dispara "When" houver algo preenchido).
        var request = new CreateCustomerDto("João da Silva", null, "", null, null, null);

        Assert.True(_validator.Validate(request).IsValid);
    }
}
