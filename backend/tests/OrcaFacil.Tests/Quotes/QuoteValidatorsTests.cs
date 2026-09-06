using OrcaFacil.Application.DTOs.Quotes;
using OrcaFacil.Application.Validators.Quotes;
using Xunit;

namespace OrcaFacil.Tests.Quotes;

public class QuoteItemInputValidatorTests
{
    private readonly QuoteItemInputValidator _validator = new();

    [Fact]
    public void Validate_ComServiceId_NaoPrecisaDeDescricaoOuPreco()
    {
        var item = new QuoteItemInputDto(Guid.NewGuid(), null, 1, null, 0);

        Assert.True(_validator.Validate(item).IsValid);
    }

    [Fact]
    public void Validate_ComDescricaoEPrecoAvulsos_DeveSerValido()
    {
        var item = new QuoteItemInputDto(null, "Cabo flexível 2,5mm", 10, 3.5m, 0);

        Assert.True(_validator.Validate(item).IsValid);
    }

    [Fact]
    public void Validate_SemServiceIdEDescricaoOuPreco_DeveGerarErro()
    {
        var item = new QuoteItemInputDto(null, null, 1, null, 0);

        Assert.False(_validator.Validate(item).IsValid);
    }

    [Fact]
    public void Validate_ComQuantidadeZero_DeveGerarErro()
    {
        var item = new QuoteItemInputDto(Guid.NewGuid(), null, 0, null, 0);

        Assert.False(_validator.Validate(item).IsValid);
    }
}

public class CreateQuoteValidatorTests
{
    private readonly CreateQuoteValidator _validator = new();

    [Fact]
    public void Validate_SemItens_DeveGerarErro()
    {
        var request = new CreateQuoteDto(Guid.NewGuid(), DateTime.UtcNow.AddDays(7), null, new List<QuoteItemInputDto>());

        Assert.False(_validator.Validate(request).IsValid);
    }

    [Fact]
    public void Validate_ComValidadeNoPassado_DeveGerarErro()
    {
        var items = new List<QuoteItemInputDto> { new(Guid.NewGuid(), null, 1, null, 0) };
        var request = new CreateQuoteDto(Guid.NewGuid(), DateTime.UtcNow.AddDays(-1), null, items);

        Assert.False(_validator.Validate(request).IsValid);
    }

    [Fact]
    public void Validate_ComDadosValidos_DeveSerValido()
    {
        var items = new List<QuoteItemInputDto> { new(Guid.NewGuid(), null, 1, null, 0) };
        var request = new CreateQuoteDto(Guid.NewGuid(), DateTime.UtcNow.AddDays(7), "Observação", items);

        Assert.True(_validator.Validate(request).IsValid);
    }
}
