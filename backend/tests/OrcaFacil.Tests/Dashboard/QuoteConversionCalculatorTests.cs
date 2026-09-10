using OrcaFacil.Application.Services;
using Xunit;

namespace OrcaFacil.Tests.Dashboard;

public class QuoteConversionCalculatorTests
{
    [Fact]
    public void CalculateRate_ComMaisAprovadosQueRejeitados_DeveCalcularPercentualCorreto()
    {
        // 8 aprovados, 2 rejeitados = 80% de conversão entre os decididos.
        var rate = QuoteConversionCalculator.CalculateRate(approvedCount: 8, rejectedCount: 2);

        Assert.Equal(80m, rate);
    }

    [Fact]
    public void CalculateRate_SemNenhumDecidido_DeveRetornarZero()
    {
        // Nada de aprovado nem rejeitado ainda (tudo em rascunho/enviado) —
        // não é "0% de conversão", é "ainda não há taxa" — mas devolvemos 0
        // em vez de dividir por zero.
        var rate = QuoteConversionCalculator.CalculateRate(approvedCount: 0, rejectedCount: 0);

        Assert.Equal(0m, rate);
    }

    [Fact]
    public void CalculateRate_TodosAprovados_DeveRetornarCem()
    {
        var rate = QuoteConversionCalculator.CalculateRate(approvedCount: 5, rejectedCount: 0);

        Assert.Equal(100m, rate);
    }

    [Fact]
    public void CalculateRate_TodosRejeitados_DeveRetornarZero()
    {
        var rate = QuoteConversionCalculator.CalculateRate(approvedCount: 0, rejectedCount: 5);

        Assert.Equal(0m, rate);
    }
}
