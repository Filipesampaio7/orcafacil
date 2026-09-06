using OrcaFacil.Domain.Entities;
using Xunit;

namespace OrcaFacil.Tests.Quotes;

public class QuoteCalculationTests
{
    [Fact]
    public void RecalculateTotals_ComUmItemSemDesconto_DeveCalcularCorretamente()
    {
        var quote = new Quote { Number = 1 };
        quote.Items.Add(new QuoteItem { Quantity = 2, UnitPrice = 100m, DiscountAmount = 0 });

        quote.RecalculateTotals();

        Assert.Equal(200m, quote.Subtotal);
        Assert.Equal(0m, quote.DiscountAmount);
        Assert.Equal(200m, quote.Total);
        Assert.Equal(200m, quote.Items.First().LineTotal);
    }

    [Fact]
    public void RecalculateTotals_ComDescontoPorItem_DeveSomarNoDescontoTotal()
    {
        var quote = new Quote { Number = 1 };
        quote.Items.Add(new QuoteItem { Quantity = 1, UnitPrice = 100m, DiscountAmount = 20m });
        quote.Items.Add(new QuoteItem { Quantity = 2, UnitPrice = 50m, DiscountAmount = 10m });

        quote.RecalculateTotals();

        // Subtotal bruto: (1*100) + (2*50) = 200
        Assert.Equal(200m, quote.Subtotal);
        // Desconto: 20 + 10 = 30
        Assert.Equal(30m, quote.DiscountAmount);
        // Total: 200 - 30 = 170
        Assert.Equal(170m, quote.Total);
    }

    [Fact]
    public void RecalculateTotals_SemItens_DeveZerarTudo()
    {
        var quote = new Quote { Number = 1 };

        quote.RecalculateTotals();

        Assert.Equal(0m, quote.Subtotal);
        Assert.Equal(0m, quote.DiscountAmount);
        Assert.Equal(0m, quote.Total);
    }
}
