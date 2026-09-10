using OrcaFacil.Application.DTOs.Dashboard;

namespace OrcaFacil.Application.Interfaces;

/// <summary>
/// Diferente dos outros repositórios (Customer, Service...), este não
/// trabalha com entidades — devolve DTOs de leitura prontos. Faz sentido
/// aqui: dashboard é puramente "consulta agregada", não algo que vai virar
/// uma entidade rastreada pelo EF Core para depois ser salva de volta.
/// </summary>
public interface IDashboardRepository
{
    Task<DashboardSummaryDto> GetSummaryAsync(Guid companyId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MonthlyAmount>> GetCompletedWorkOrderRevenueByMonthAsync(
        Guid companyId, DateTime sinceUtc, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MonthlyAmount>> GetApprovedQuoteRevenueByMonthAsync(
        Guid companyId, DateTime sinceUtc, CancellationToken cancellationToken = default);

    Task<QuoteStatusCounts> GetQuoteStatusCountsAsync(
        Guid companyId, DateTime? sinceUtc, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ServiceRankingDto>> GetServiceRankingAsync(
        Guid companyId, int take, CancellationToken cancellationToken = default);
}

public record MonthlyAmount(int Year, int Month, decimal Amount);

public record QuoteStatusCounts(int Draft, int Sent, int Approved, int Rejected, int Expired, int Cancelled)
{
    public int Total => Draft + Sent + Approved + Rejected + Expired + Cancelled;
}
