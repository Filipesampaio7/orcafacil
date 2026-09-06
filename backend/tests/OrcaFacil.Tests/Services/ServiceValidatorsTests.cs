using OrcaFacil.Application.DTOs.Services;
using OrcaFacil.Application.Validators.Services;
using OrcaFacil.Domain.Enums;
using Xunit;

namespace OrcaFacil.Tests.Services;

public class CreateServiceValidatorTests
{
    private readonly CreateServiceValidator _validator = new();

    [Fact]
    public void Validate_ComDadosValidos_DeveSerValido()
    {
        var request = new CreateServiceDto("Troca de óleo", "Descrição", 150m, 60, "Manutenção");

        Assert.True(_validator.Validate(request).IsValid);
    }

    [Fact]
    public void Validate_ComPrecoNegativo_DeveGerarErro()
    {
        var request = new CreateServiceDto("Troca de óleo", null, -10m, null, null);

        Assert.False(_validator.Validate(request).IsValid);
    }

    [Fact]
    public void Validate_ComDuracaoZero_DeveGerarErro()
    {
        var request = new CreateServiceDto("Troca de óleo", null, 150m, 0, null);

        Assert.False(_validator.Validate(request).IsValid);
    }

    [Fact]
    public void Validate_SemDuracaoInformada_DeveSerValido()
    {
        // Duração é opcional — null não deve disparar a regra de "maior que zero".
        var request = new CreateServiceDto("Troca de óleo", null, 150m, null, null);

        Assert.True(_validator.Validate(request).IsValid);
    }
}

public class UpdateServiceValidatorTests
{
    private readonly UpdateServiceValidator _validator = new();

    [Fact]
    public void Validate_ComStatusValido_DeveSerValido()
    {
        var request = new UpdateServiceDto("Troca de óleo", null, 150m, 60, null, ServiceStatus.Inactive);

        Assert.True(_validator.Validate(request).IsValid);
    }
}
