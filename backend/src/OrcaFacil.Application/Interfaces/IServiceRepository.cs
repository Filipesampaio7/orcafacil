using OrcaFacil.Domain.Entities;
using OrcaFacil.Domain.Enums;

namespace OrcaFacil.Application.Interfaces;

public interface IServiceRepository : IRepository<Service>
{
    Task<Service?> GetByIdForCompanyAsync(Guid id, Guid companyId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Service>> SearchAsync(
        Guid companyId, string? search, string? category, ServiceStatus? status, CancellationToken cancellationToken = default);
}
