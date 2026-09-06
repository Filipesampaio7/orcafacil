using OrcaFacil.Application.DTOs.Services;
using OrcaFacil.Domain.Enums;

namespace OrcaFacil.Application.Interfaces;

/// <summary>
/// Chamado "ServiceCatalogService" (não "ServiceService") de propósito —
/// evita confundir "Service" a entidade de domínio com "Service" o sufixo
/// convencional de classe de regra de negócio nesta camada.
/// </summary>
public interface IServiceCatalogService
{
    Task<IReadOnlyList<ServiceResponseDto>> GetAllAsync(
        string? search, string? category, ServiceStatus? status, CancellationToken cancellationToken = default);
    Task<ServiceResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ServiceResponseDto> CreateAsync(CreateServiceDto request, CancellationToken cancellationToken = default);
    Task<ServiceResponseDto> UpdateAsync(Guid id, UpdateServiceDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
