using Microsoft.EntityFrameworkCore;
using OrcaFacil.Application.Interfaces;
using OrcaFacil.Domain.Entities;
using OrcaFacil.Domain.Enums;
using OrcaFacil.Infrastructure.Data;

namespace OrcaFacil.Infrastructure.Repositories;

public class WorkOrderRepository : Repository<WorkOrder>, IWorkOrderRepository
{
    private readonly AppDbContext _context;

    public WorkOrderRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public Task<WorkOrder?> GetByIdForCompanyAsync(Guid id, Guid companyId, CancellationToken cancellationToken = default) =>
        _context.WorkOrders
            .Include(w => w.Customer)
            .Include(w => w.AssignedUser)
            .Include(w => w.Quote)
            .Include(w => w.Items)
            .FirstOrDefaultAsync(w => w.Id == id && w.CompanyId == companyId, cancellationToken);

    public async Task<IReadOnlyList<WorkOrder>> SearchAsync(
        Guid companyId, WorkOrderStatus? status, Guid? customerId, Guid? assignedUserId,
        CancellationToken cancellationToken = default)
    {
        var query = _context.WorkOrders
            .Include(w => w.Customer)
            .Include(w => w.AssignedUser)
            .Include(w => w.Items)
            .Where(w => w.CompanyId == companyId);

        if (status.HasValue)
        {
            query = query.Where(w => w.Status == status.Value);
        }

        if (customerId.HasValue)
        {
            query = query.Where(w => w.CustomerId == customerId.Value);
        }

        if (assignedUserId.HasValue)
        {
            query = query.Where(w => w.AssignedUserId == assignedUserId.Value);
        }

        return await query.OrderByDescending(w => w.CreatedAt).ToListAsync(cancellationToken);
    }
}
