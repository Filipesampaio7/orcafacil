using OrcaFacil.Domain.Common;
using OrcaFacil.Domain.Enums;

namespace OrcaFacil.Domain.Entities;

public class Service : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal DefaultPrice { get; set; }
    public int? EstimatedDurationMinutes { get; set; }
    public string? Category { get; set; }
    public ServiceStatus Status { get; set; } = ServiceStatus.Active;

    public ICollection<QuoteItem> QuoteItems { get; set; } = new List<QuoteItem>();
    public ICollection<WorkOrderItem> WorkOrderItems { get; set; } = new List<WorkOrderItem>();
}
