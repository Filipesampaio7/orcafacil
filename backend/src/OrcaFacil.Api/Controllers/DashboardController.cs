using Microsoft.AspNetCore.Mvc;
using OrcaFacil.Application.DTOs.Dashboard;
using OrcaFacil.Application.Interfaces;

namespace OrcaFacil.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>Números gerais: orçamentos totais/pendentes/aprovados, O.S. em andamento/concluídas, faturamento estimado/realizado.</summary>
    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary(CancellationToken cancellationToken)
    {
        return Ok(await _dashboardService.GetSummaryAsync(cancellationToken));
    }

    /// <summary>Série de faturamento mensal (realizado via O.S. concluídas + estimado via orçamentos aprovados) dos últimos N meses (padrão 6).</summary>
    [HttpGet("revenue")]
    public async Task<ActionResult<IReadOnlyList<MonthlyRevenueDto>>> GetRevenue(
        [FromQuery] int months, CancellationToken cancellationToken)
    {
        return Ok(await _dashboardService.GetMonthlyRevenueAsync(months == 0 ? 6 : months, cancellationToken));
    }

    /// <summary>Taxa de conversão de orçamentos (aprovados vs. rejeitados/pendentes). Sem "months", considera todo o histórico.</summary>
    [HttpGet("quote-conversion")]
    public async Task<ActionResult<QuoteConversionDto>> GetQuoteConversion(
        [FromQuery] int? months, CancellationToken cancellationToken)
    {
        return Ok(await _dashboardService.GetQuoteConversionAsync(months, cancellationToken));
    }

    /// <summary>Ranking de serviços do catálogo executados em O.S. concluídas, ordenado por faturamento (padrão: top 10).</summary>
    [HttpGet("service-ranking")]
    public async Task<ActionResult<IReadOnlyList<ServiceRankingDto>>> GetServiceRanking(
        [FromQuery] int take, CancellationToken cancellationToken)
    {
        return Ok(await _dashboardService.GetServiceRankingAsync(take == 0 ? 10 : take, cancellationToken));
    }
}
