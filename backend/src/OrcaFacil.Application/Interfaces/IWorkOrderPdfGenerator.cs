namespace OrcaFacil.Application.Interfaces;

/// <summary>Mesmo raciocínio do IQuotePdfGenerator: a Application monta os dados, quem implementa só desenha.</summary>
public interface IWorkOrderPdfGenerator
{
    byte[] Generate(WorkOrderPdfData data);
}

public record WorkOrderPdfData(
    string CompanyName,
    string? CompanyCnpj,
    string? CompanyPhone,
    string? CompanyEmail,
    string? CompanyAddress,
    string? CompanyLogoPath,
    Guid Id,
    int? QuoteNumber,
    DateTime CreatedAt,
    DateTime? ScheduledDate,
    string Status,
    string CustomerName,
    string? CustomerPhone,
    string? CustomerEmail,
    string? CustomerAddress,
    string? AssignedUserName,
    IReadOnlyList<WorkOrderPdfItem> Items,
    decimal Total,
    string? Notes,
    string? TechnicalObservations);

public record WorkOrderPdfItem(string Description, decimal Quantity, decimal UnitPrice, decimal LineTotal);
