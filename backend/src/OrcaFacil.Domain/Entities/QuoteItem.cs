using OrcaFacil.Domain.Common;

namespace OrcaFacil.Domain.Entities;

/// <summary>
/// ServiceId é opcional porque o orçamento também aceita "produtos/materiais"
/// avulsos, sem cadastro prévio em Service. Description é sempre preenchida —
/// quando vem de um Service, é uma cópia (snapshot) do nome no momento da
/// criação do orçamento, para que renomear o serviço depois não altere
/// orçamentos já emitidos.
/// </summary>
public class QuoteItem : BaseEntity
{
    public Guid QuoteId { get; set; }
    public Quote Quote { get; set; } = null!;

    public Guid? ServiceId { get; set; }
    public Service? Service { get; set; }

    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal LineTotal { get; set; }

    /// <summary>(Quantidade × Preço unitário) − Desconto do item.</summary>
    public void Recalculate()
    {
        LineTotal = (Quantity * UnitPrice) - DiscountAmount;
    }
}
