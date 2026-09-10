using OrcaFacil.Application.DTOs.Dashboard;

namespace OrcaFacil.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MonthlyRevenueDto>> GetMonthlyRevenueAsync(int months, CancellationToken cancellationToken = default);
    Task<QuoteConversionDto> GetQuoteConversionAsync(int? months, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ServiceRankingDto>> GetServiceRankingAsync(int take, CancellationToken cancellationToken = default);
}
