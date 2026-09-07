using OrcaFacil.Application.Services;
using Xunit;

namespace OrcaFacil.Tests.Pdf;

public class WhatsAppMessageBuilderTests
{
    [Fact]
    public void Build_ComTelefone_DeveGerarLinkWaMe()
    {
        var result = WhatsAppMessageBuilder.Build(
            "Maria Silva", "(11) 99999-8888", 12, 350.5m, new DateTime(2026, 12, 31));

        Assert.Contains("Maria Silva", result.Message);
        Assert.Contains("#0012", result.Message);
        Assert.Contains("350,50", result.Message);
        Assert.Contains("31/12/2026", result.Message);

        Assert.NotNull(result.WhatsAppLink);
        Assert.StartsWith("https://wa.me/11999998888?text=", result.WhatsAppLink);
    }

    [Fact]
    public void Build_SemTelefone_LinkDeveSerNulo()
    {
        var result = WhatsAppMessageBuilder.Build(
            "Maria Silva", null, 12, 350.5m, new DateTime(2026, 12, 31));

        Assert.Null(result.WhatsAppLink);
        Assert.Contains("Maria Silva", result.Message);
    }

    [Fact]
    public void Build_ComTelefoneSoComLetras_LinkDeveSerNulo()
    {
        var result = WhatsAppMessageBuilder.Build(
            "Maria Silva", "não informado", 12, 350.5m, new DateTime(2026, 12, 31));

        Assert.Null(result.WhatsAppLink);
    }
}
