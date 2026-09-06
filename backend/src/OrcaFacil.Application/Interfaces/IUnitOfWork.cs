namespace OrcaFacil.Application.Interfaces;

/// <summary>
/// Conceito-chave: IRepository monta as mudanças em memória (Add/Update/Remove
/// só marcam o estado no ChangeTracker do EF Core); nada é persistido até
/// SaveChangesAsync ser chamado aqui. Isso permite que um Service combine
/// várias operações de repositórios diferentes numa única transação — ex.:
/// "aprovar orçamento" pode atualizar o Quote E criar um WorkOrder, e as duas
/// mudanças só vão para o banco juntas, na FASE 6/8.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
