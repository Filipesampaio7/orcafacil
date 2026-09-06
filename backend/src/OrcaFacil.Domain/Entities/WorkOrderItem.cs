using OrcaFacil.Domain.Common;

namespace OrcaFacil.Domain.Entities;

public class WorkOrderItem : BaseEntity
{
    public Guid WorkOrderId { get; set; }
    public WorkOrder WorkOrder { get; set; } = null!;

    public Guid? ServiceId { get; set; }
    public Service? Service { get; set; }

    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}
