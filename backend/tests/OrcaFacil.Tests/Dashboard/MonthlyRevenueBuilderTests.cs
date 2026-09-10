using OrcaFacil.Application.Interfaces;
using OrcaFacil.Application.Services;
using Xunit;

namespace OrcaFacil.Tests.Dashboard;

public class MonthlyRevenueBuilderTests
{
    private static readonly DateTime Reference = new(2026, 3, 15, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Build_DeveRetornarUmPontoPorMesSolicitado()
    {
        var empty = new List<MonthlyAmount>();
        var result = MonthlyRevenueBuilder.Build(3, Reference, empty, empty);

        Assert.Equal(3, result.Count);
        // Ordem cronológica: mais antigo primeiro, mês de referência por último.
        Assert.Equal((2026, 1), (result[0].Year, result[0].Month));
        Assert.Equal((2026, 2), (result[1].Year, result[1].Month));
        Assert.Equal((2026, 3), (result[2].Year, result[2].Month));
    }

    [Fact]
    public void Build_MesSemDados_DeveVirZeroEmVezDeSumir()
    {
        // Só um mês com dado (fevereiro) — janeiro e março precisam aparecer com 0.
        var realized = new List<MonthlyAmount> { new(2026, 2, 500m) };
        var noEstimated = new List<MonthlyAmount>();

        var result = MonthlyRevenueBuilder.Build(3, Reference, realized, noEstimated);

        Assert.Equal(0m, result[0].RealizedRevenue); // janeiro
        Assert.Equal(500m, result[1].RealizedRevenue); // fevereiro
        Assert.Equal(0m, result[2].RealizedRevenue); // março
    }

    [Fact]
    public void Build_RealizadoEEstimado_DevemSerIndependentes()
    {
        var realized = new List<MonthlyAmount> { new(2026, 3, 300m) };
        var estimated = new List<MonthlyAmount> { new(2026, 3, 450m) };

        var result = MonthlyRevenueBuilder.Build(1, Reference, realized, estimated);

        Assert.Equal(300m, result[0].RealizedRevenue);
        Assert.Equal(450m, result[0].EstimatedRevenue);
    }
}
