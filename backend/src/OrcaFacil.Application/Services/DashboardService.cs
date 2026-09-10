using OrcaFacil.Application.DTOs.Dashboard;
using OrcaFacil.Application.Exceptions;
using OrcaFacil.Application.Interfaces;

namespace OrcaFacil.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _dashboardRepository;
    private readonly ICurrentUserService _currentUserService;

    public DashboardService(IDashboardRepository dashboardRepository, ICurrentUserService currentUserService)
    {
        _dashboardRepository = dashboardRepository;
        _currentUserService = currentUserService;
    }

    public Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default) =>
        _dashboardRepository.GetSummaryAsync(CompanyId, cancellationToken);

    public async Task<IReadOnlyList<MonthlyRevenueDto>> GetMonthlyRevenueAsync(
        int months, CancellationToken cancellationToken = default)
    {
        months = Math.Clamp(months, 1, 24);
        var now = DateTime.UtcNow;
        var since = FirstOfMonth(now).AddMonths(-(months - 1));

        var realized = await _dashboardRepository.GetCompletedWorkOrderRevenueByMonthAsync(CompanyId, since, cancellationToken);
        var estimated = await _dashboardRepository.GetApprovedQuoteRevenueByMonthAsync(CompanyId, since, cancellationToken);

        return MonthlyRevenueBuilder.Build(months, now, realized, estimated);
    }

    public async Task<QuoteConversionDto> GetQuoteConversionAsync(int? months, CancellationToken cancellationToken = default)
    {
        DateTime? since = null;
        if (months.HasValue)
        {
            since = FirstOfMonth(DateTime.UtcNow).AddMonths(-(Math.Max(months.Value, 1) - 1));
        }

        var counts = await _dashboardRepository.GetQuoteStatusCountsAsync(CompanyId, since, cancellationToken);
        var rate = QuoteConversionCalculator.CalculateRate(counts.Approved, counts.Rejected);

        return new QuoteConversionDto(
            counts.Total, counts.Draft, counts.Sent, counts.Approved, counts.Rejected, counts.Expired, counts.Cancelled, rate);
    }

    public Task<IReadOnlyList<ServiceRankingDto>> GetServiceRankingAsync(
        int take, CancellationToken cancellationToken = default) =>
        _dashboardRepository.GetServiceRankingAsync(CompanyId, Math.Clamp(take, 1, 100), cancellationToken);

    private static DateTime FirstOfMonth(DateTime value) =>
        new(value.Year, value.Month, 1, 0, 0, 0, DateTimeKind.Utc);

    private Guid CompanyId => _currentUserService.CompanyId
        ?? throw new InvalidCredentialsException();
}
