using OrcaFacil.Domain.Entities;
using OrcaFacil.Domain.Enums;

namespace OrcaFacil.Application.Interfaces;

public interface IWorkOrderRepository : IRepository<WorkOrder>
{
    Task<WorkOrder?> GetByIdForCompanyAsync(Guid id, Guid companyId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WorkOrder>> SearchAsync(
        Guid companyId, WorkOrderStatus? status, Guid? customerId, Guid? assignedUserId,
        CancellationToken cancellationToken = default);
}
