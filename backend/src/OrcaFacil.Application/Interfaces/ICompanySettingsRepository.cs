using OrcaFacil.Domain.Entities;

namespace OrcaFacil.Application.Interfaces;

public interface ICompanySettingsRepository : IRepository<CompanySettings>
{
    Task<CompanySettings?> GetByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default);
}
