using OrcaFacil.Application.DTOs.WorkOrders;
using OrcaFacil.Domain.Enums;

namespace OrcaFacil.Application.Interfaces;

public interface IWorkOrderService
{
    Task<IReadOnlyList<WorkOrderSummaryDto>> GetAllAsync(
        WorkOrderStatus? status, Guid? customerId, Guid? assignedUserId, CancellationToken cancellationToken = default);
    Task<WorkOrderResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<WorkOrderResponseDto> CreateAsync(CreateWorkOrderDto request, CancellationToken cancellationToken = default);
    Task<WorkOrderResponseDto> ConvertFromQuoteAsync(Guid quoteId, CancellationToken cancellationToken = default);
    Task<WorkOrderResponseDto> UpdateAsync(Guid id, UpdateWorkOrderDto request, CancellationToken cancellationToken = default);
    Task<WorkOrderResponseDto> UpdateStatusAsync(Guid id, WorkOrderStatus newStatus, CancellationToken cancellationToken = default);
}
