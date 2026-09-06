using Microsoft.EntityFrameworkCore;
using OrcaFacil.Application.Interfaces;
using OrcaFacil.Domain.Entities;
using OrcaFacil.Domain.Enums;
using OrcaFacil.Infrastructure.Data;

namespace OrcaFacil.Infrastructure.Repositories;

public class ServiceRepository : Repository<Service>, IServiceRepository
{
    private readonly AppDbContext _context;

    public ServiceRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public Task<Service?> GetByIdForCompanyAsync(Guid id, Guid companyId, CancellationToken cancellationToken = default) =>
        _context.Services.FirstOrDefaultAsync(s => s.Id == id && s.CompanyId == companyId, cancellationToken);

    public async Task<IReadOnlyList<Service>> SearchAsync(
        Guid companyId, string? search, string? category, ServiceStatus? status, CancellationToken cancellationToken = default)
    {
        var query = _context.Services.Where(s => s.CompanyId == companyId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(s => s.Name.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(s => s.Category == category);
        }

        if (status.HasValue)
        {
            query = query.Where(s => s.Status == status.Value);
        }

        return await query.OrderBy(s => s.Name).ToListAsync(cancellationToken);
    }
}
