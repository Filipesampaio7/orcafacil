using OrcaFacil.Domain.Enums;

namespace OrcaFacil.Application.DTOs.WorkOrders;

/// <summary>Mesma regra do QuoteItemInputDto: ServiceId OU (Description + UnitPrice). Sem desconto — O.S. registra execução, não negociação.</summary>
public record WorkOrderItemInputDto(
    Guid? ServiceId,
    string? Description,
    decimal Quantity,
    decimal? UnitPrice);

public record CreateWorkOrderDto(
    Guid CustomerId,
    Guid? AssignedUserId,
    DateTime? ScheduledDate,
    string? Notes,
    List<WorkOrderItemInputDto> Items);

public record UpdateWorkOrderDto(
    Guid? AssignedUserId,
    DateTime? ScheduledDate,
    string? Notes,
    List<WorkOrderItemInputDto> Items);

public record UpdateWorkOrderStatusDto(WorkOrderStatus Status);

public record WorkOrderItemResponseDto(
    Guid Id,
    Guid? ServiceId,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal LineTotal);

public record WorkOrderSummaryDto(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    Guid? AssignedUserId,
    string? AssignedUserName,
    WorkOrderStatus Status,
    DateTime? ScheduledDate,
    decimal Total);

public record WorkOrderResponseDto(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    Guid? QuoteId,
    int? QuoteNumber,
    Guid? AssignedUserId,
    string? AssignedUserName,
    WorkOrderStatus Status,
    DateTime? ScheduledDate,
    string? Notes,
    decimal Total,
    IReadOnlyList<WorkOrderItemResponseDto> Items);
