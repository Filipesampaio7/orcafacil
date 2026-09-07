using Microsoft.EntityFrameworkCore;
using OrcaFacil.Application.Interfaces;
using OrcaFacil.Domain.Entities;
using OrcaFacil.Infrastructure.Data;

namespace OrcaFacil.Infrastructure.Repositories;

public class CompanySettingsRepository : Repository<CompanySettings>, ICompanySettingsRepository
{
    private readonly AppDbContext _context;

    public CompanySettingsRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public Task<CompanySettings?> GetByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        _context.CompanySettings.FirstOrDefaultAsync(s => s.CompanyId == companyId, cancellationToken);
}
