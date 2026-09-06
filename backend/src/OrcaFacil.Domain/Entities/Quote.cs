using OrcaFacil.Domain.Common;
using OrcaFacil.Domain.Enums;

namespace OrcaFacil.Domain.Entities;

/// <summary>
/// Subtotal/DiscountAmount/Total ficam armazenados (não só calculados on-the-fly)
/// porque um orçamento é um documento histórico: se o preço de um serviço mudar
/// depois, o orçamento já enviado ao cliente não pode mudar de valor sozinho.
/// O cálculo em si (a partir dos QuoteItems) é responsabilidade do
/// QuoteService, na FASE 6.
/// </summary>
public class Quote : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    /// <summary>Número sequencial exibido ao cliente (#0001), único por empresa.</summary>
    public int Number { get; set; }

    public QuoteStatus Status { get; set; } = QuoteStatus.Draft;
    public DateTime IssueDate { get; set; } = DateTime.UtcNow;
    public DateTime ValidUntil { get; set; }

    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal Total { get; set; }

    public string? Notes { get; set; }

    public ICollection<QuoteItem> Items { get; set; } = new List<QuoteItem>();
    public WorkOrder? WorkOrder { get; set; }

    /// <summary>
    /// Recalcula LineTotal de cada item e os totais do orçamento a partir
    /// deles. Fica na entidade (não num Service da Application) para não
    /// haver dois lugares diferentes — criar e editar — calculando isso de
    /// formas ligeiramente diferentes com o tempo.
    /// </summary>
    public void RecalculateTotals()
    {
        foreach (var item in Items)
        {
            item.Recalculate();
        }

        Subtotal = Items.Sum(i => i.Quantity * i.UnitPrice);
        DiscountAmount = Items.Sum(i => i.DiscountAmount);
        Total = Items.Sum(i => i.LineTotal);
    }
}
