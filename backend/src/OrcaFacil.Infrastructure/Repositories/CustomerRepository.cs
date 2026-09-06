using Microsoft.EntityFrameworkCore;
using OrcaFacil.Application.Interfaces;
using OrcaFacil.Domain.Entities;
using OrcaFacil.Infrastructure.Data;

namespace OrcaFacil.Infrastructure.Repositories;

public class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    private readonly AppDbContext _context;

    public CustomerRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public Task<Customer?> GetByIdForCompanyAsync(Guid id, Guid companyId, CancellationToken cancellationToken = default) =>
        _context.Customers.FirstOrDefaultAsync(c => c.Id == id && c.CompanyId == companyId, cancellationToken);

    public async Task<IReadOnlyList<Customer>> SearchAsync(
        Guid companyId, string? search, CancellationToken cancellationToken = default)
    {
        var query = _context.Customers.Where(c => c.CompanyId == companyId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            // Contains() vira "LIKE '%valor%'" no SQL — no SQLite isso é
            // case-insensitive para caracteres ASCII por padrão; em outro
            // provider (ex.: PostgreSQL) pode não ser. Se isso importar no
            // futuro, o ajuste é usar EF.Functions.ILike (Postgres) aqui.
            query = query.Where(c =>
                c.Name.Contains(search) ||
                (c.Phone != null && c.Phone.Contains(search)) ||
                (c.Email != null && c.Email.Contains(search)) ||
                (c.Document != null && c.Document.Contains(search)));
        }

        return await query.OrderBy(c => c.Name).ToListAsync(cancellationToken);
    }
}
