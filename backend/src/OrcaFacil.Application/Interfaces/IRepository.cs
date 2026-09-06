using OrcaFacil.Domain.Common;

namespace OrcaFacil.Application.Interfaces;

/// <summary>
/// Contrato de acesso a dados que a Application enxerga — sem saber que por
/// baixo existe Entity Framework Core ou PostgreSQL. A implementação real
/// mora em OrcaFacil.Infrastructure.Repositories (FASE 2).
///
/// Conceito-chave para estudar: "Dependency Inversion" — a camada de mais
/// alto nível (Application) define a interface; a de mais baixo nível
/// (Infrastructure) depende dela, e não o contrário.
/// </summary>
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Remove(T entity);
}
