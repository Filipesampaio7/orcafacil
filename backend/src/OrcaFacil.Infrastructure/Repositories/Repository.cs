using Microsoft.EntityFrameworkCore;
using OrcaFacil.Application.Interfaces;
using OrcaFacil.Domain.Common;
using OrcaFacil.Infrastructure.Data;

namespace OrcaFacil.Infrastructure.Repositories;

/// <summary>
/// Implementação única que serve qualquer entidade (Customer, Service, Quote...).
/// Consultas específicas de uma entidade (ex.: "clientes cujo nome contém X")
/// não entram aqui — ficam num repositório dedicado (ex.: ICustomerRepository)
/// quando a funcionalidade específica precisar, a partir da FASE 4.
/// </summary>
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    private readonly AppDbContext _context;
    private readonly DbSet<T> _dbSet;

    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbSet.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _dbSet.ToListAsync(cancellationToken);

    public Task AddAsync(T entity, CancellationToken cancellationToken = default) =>
        _dbSet.AddAsync(entity, cancellationToken).AsTask();

    public void Update(T entity) => _dbSet.Update(entity);

    public void Remove(T entity) => _dbSet.Remove(entity);
}
