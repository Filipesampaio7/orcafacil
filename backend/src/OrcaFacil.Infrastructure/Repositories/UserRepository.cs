using Microsoft.EntityFrameworkCore;
using OrcaFacil.Application.Interfaces;
using OrcaFacil.Domain.Entities;
using OrcaFacil.Infrastructure.Data;

namespace OrcaFacil.Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        _context.Users
            .Include(u => u.Company)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
}
