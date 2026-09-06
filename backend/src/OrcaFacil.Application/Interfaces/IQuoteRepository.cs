using OrcaFacil.Domain.Entities;
using OrcaFacil.Domain.Enums;

namespace OrcaFacil.Application.Interfaces;

public interface IQuoteRepository : IRepository<Quote>
{
    Task<Quote?> GetByIdForCompanyAsync(Guid id, Guid companyId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Quote>> SearchAsync(
        Guid companyId, QuoteStatus? status, Guid? customerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// MAX(Number) + 1 para a empresa. Simplificação válida para uso com um
    /// usuário criando orçamentos por vez; sob criação concorrente de dois
    /// orçamentos da mesma empresa ao mesmo tempo, existe uma janela de corrida
    /// possível. Para produção com múltiplos usuários simultâneos na mesma
    /// empresa, o ideal é uma tabela de contador com lock, fora do escopo do MVP.
    /// </summary>
    Task<int> GetNextNumberAsync(Guid companyId, CancellationToken cancellationToken = default);
}
