using OrcaFacil.Domain.Common;
using OrcaFacil.Domain.Enums;

namespace OrcaFacil.Domain.Entities;

public class WorkOrder : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    /// <summary>Nulo se a O.S. for criada diretamente, sem passar por um orçamento.</summary>
    public Guid? QuoteId { get; set; }
    public Quote? Quote { get; set; }

    public Guid? AssignedUserId { get; set; }
    public User? AssignedUser { get; set; }

    public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Awaiting;
    public DateTime? ScheduledDate { get; set; }
    public string? Notes { get; set; }

    /// <summary>Mesma finalidade da versão em Quote — ver comentário lá.</summary>
    public string? TechnicalObservations { get; set; }

    public ICollection<WorkOrderItem> Items { get; set; } = new List<WorkOrderItem>();
}
