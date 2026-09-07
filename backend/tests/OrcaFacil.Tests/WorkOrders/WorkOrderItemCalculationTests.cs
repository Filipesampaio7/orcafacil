using OrcaFacil.Domain.Entities;
using Xunit;

namespace OrcaFacil.Tests.WorkOrders;

public class WorkOrderItemCalculationTests
{
    [Fact]
    public void Recalculate_DeveMultiplicarQuantidadePorPrecoSemDesconto()
    {
        var item = new WorkOrderItem { Quantity = 3, UnitPrice = 80m };

        item.Recalculate();

        Assert.Equal(240m, item.LineTotal);
    }
}
