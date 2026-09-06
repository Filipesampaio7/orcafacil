using OrcaFacil.Domain.Enums;

namespace OrcaFacil.Application.DTOs.Services;

public record CreateServiceDto(
    string Name,
    string? Description,
    decimal DefaultPrice,
    int? EstimatedDurationMinutes,
    string? Category);

public record UpdateServiceDto(
    string Name,
    string? Description,
    decimal DefaultPrice,
    int? EstimatedDurationMinutes,
    string? Category,
    ServiceStatus Status);

public record ServiceResponseDto(
    Guid Id,
    string Name,
    string? Description,
    decimal DefaultPrice,
    int? EstimatedDurationMinutes,
    string? Category,
    ServiceStatus Status,
    DateTime CreatedAt);
