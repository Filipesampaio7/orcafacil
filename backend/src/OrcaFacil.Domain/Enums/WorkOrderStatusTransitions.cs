namespace OrcaFacil.Domain.Enums;

/// <summary>Mesmo raciocínio de QuoteStatusTransitions: regra de negócio pura, testável sem banco nem HTTP.</summary>
public static class WorkOrderStatusTransitions
{
    private static readonly Dictionary<WorkOrderStatus, WorkOrderStatus[]> Allowed = new()
    {
        [WorkOrderStatus.Awaiting] = new[] { WorkOrderStatus.Scheduled, WorkOrderStatus.InProgress, WorkOrderStatus.Cancelled },
        [WorkOrderStatus.Scheduled] = new[] { WorkOrderStatus.InProgress, WorkOrderStatus.Cancelled },
        [WorkOrderStatus.InProgress] = new[] { WorkOrderStatus.Completed, WorkOrderStatus.Cancelled },
        [WorkOrderStatus.Completed] = Array.Empty<WorkOrderStatus>(),
        [WorkOrderStatus.Cancelled] = Array.Empty<WorkOrderStatus>(),
    };

    public static bool CanTransition(WorkOrderStatus from, WorkOrderStatus to) =>
        Allowed.TryGetValue(from, out var next) && next.Contains(to);
}
