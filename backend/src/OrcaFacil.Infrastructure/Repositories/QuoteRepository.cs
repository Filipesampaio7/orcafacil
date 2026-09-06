using Microsoft.EntityFrameworkCore;
using OrcaFacil.Application.Interfaces;
using OrcaFacil.Domain.Entities;
using OrcaFacil.Domain.Enums;
using OrcaFacil.Infrastructure.Data;

namespace OrcaFacil.Infrastructure.Repositories;

public class QuoteRepository : Repository<Quote>, IQuoteRepository
{
    private readonly AppDbContext _context;

    public QuoteRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public Task<Quote?> GetByIdForCompanyAsync(Guid id, Guid companyId, CancellationToken cancellationToken = default) =>
        _context.Quotes
            .Include(q => q.Customer)
            .Include(q => q.Items)
            .FirstOrDefaultAsync(q => q.Id == id && q.CompanyId == companyId, cancellationToken);

    public async Task<IReadOnlyList<Quote>> SearchAsync(
        Guid companyId, QuoteStatus? status, Guid? customerId, CancellationToken cancellationToken = default)
    {
        var query = _context.Quotes.Include(q => q.Customer).Where(q => q.CompanyId == companyId);

        if (status.HasValue)
        {
            query = query.Where(q => q.Status == status.Value);
        }

        if (customerId.HasValue)
        {
            query = query.Where(q => q.CustomerId == customerId.Value);
        }

        return await query.OrderByDescending(q => q.Number).ToListAsync(cancellationToken);
    }

    public async Task<int> GetNextNumberAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var maxNumber = await _context.Quotes
            .Where(q => q.CompanyId == companyId)
            .Select(q => (int?)q.Number)
            .MaxAsync(cancellationToken);

        return (maxNumber ?? 0) + 1;
    }
}
