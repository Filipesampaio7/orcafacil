using OrcaFacil.Domain.Entities;
using OrcaFacil.Domain.Enums;
using Xunit;

namespace OrcaFacil.Tests.Domain;

public class EntityRelationshipTests
{
    [Fact]
    public void QuoteItem_DeveReferenciarQuoteCorretamente()
    {
        var quote = new Quote { Number = 1 };
        var item = new QuoteItem
        {
            Quote = quote,
            QuoteId = quote.Id,
            Description = "Instalação de ponto elétrico",
            Quantity = 2,
            UnitPrice = 150m,
        };

        quote.Items.Add(item);

        Assert.Single(quote.Items);
        Assert.Equal(quote.Id, item.QuoteId);
        Assert.Same(quote, item.Quote);
    }

    [Fact]
    public void Service_DeveComecarComoAtivoPorPadrao()
    {
        var service = new Service { Name = "Manutenção preventiva", DefaultPrice = 200m };

        Assert.Equal(ServiceStatus.Active, service.Status);
    }

    [Fact]
    public void Quote_DeveComecarComoRascunhoPorPadrao()
    {
        var quote = new Quote { Number = 1 };

        Assert.Equal(QuoteStatus.Draft, quote.Status);
    }

    [Fact]
    public void WorkOrder_DeveComecarComoAguardandoPorPadrao()
    {
        var workOrder = new WorkOrder();

        Assert.Equal(WorkOrderStatus.Awaiting, workOrder.Status);
    }

    [Fact]
    public void QuoteItem_ServiceIdDeveSerOpcional()
    {
        // Item de orçamento sem serviço cadastrado — "produto/material avulso".
        var item = new QuoteItem
        {
            Description = "Cabo flexível 2,5mm (10m)",
            Quantity = 10,
            UnitPrice = 3.5m,
        };

        Assert.Null(item.ServiceId);
        Assert.Null(item.Service);
    }
}
