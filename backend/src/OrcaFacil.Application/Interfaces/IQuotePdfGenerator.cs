namespace OrcaFacil.Application.Interfaces;

/// <summary>
/// A Application monta os dados (juntando Quote + Customer + Company +
/// CompanySettings, já resolvidos) e entrega prontos aqui — quem implementa
/// isto (QuestPdfQuoteGenerator, em Infrastructure) só sabe desenhar um PDF a
/// partir de texto e números, sem tocar em repositório ou EF Core nenhum.
/// </summary>
public interface IQuotePdfGenerator
{
    byte[] Generate(QuotePdfData data);
}

public record QuotePdfData(
    string CompanyName,
    string? CompanyCnpj,
    string? CompanyPhone,
    string? CompanyEmail,
    string? CompanyAddress,
    string? CompanyLogoPath,
    int Number,
    DateTime IssueDate,
    DateTime ValidUntil,
    string CustomerName,
    string? CustomerPhone,
    string? CustomerEmail,
    string? CustomerAddress,
    IReadOnlyList<QuotePdfItem> Items,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal Total,
    string? Notes,
    string? TechnicalObservations);

public record QuotePdfItem(string Description, decimal Quantity, decimal UnitPrice, decimal DiscountAmount, decimal LineTotal);
