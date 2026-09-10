using Microsoft.EntityFrameworkCore;
using OrcaFacil.Application.DTOs.Dashboard;
using OrcaFacil.Application.Interfaces;
using OrcaFacil.Domain.Enums;
using OrcaFacil.Infrastructure.Data;

namespace OrcaFacil.Infrastructure.Repositories;

public class DashboardRepository : IDashboardRepository
{
    private readonly AppDbContext _context;

    public DashboardRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var totalQuotes = await _context.Quotes
            .CountAsync(q => q.CompanyId == companyId, cancellationToken);
        var pendingQuotes = await _context.Quotes
            .CountAsync(q => q.CompanyId == companyId && q.Status == QuoteStatus.Sent, cancellationToken);
        var approvedQuotes = await _context.Quotes
            .CountAsync(q => q.CompanyId == companyId && q.Status == QuoteStatus.Approved, cancellationToken);
        // SumAsync direto em decimal não funciona no provider SQLite do EF
        // Core: "SQLite cannot apply aggregate operator 'Sum' on
        // expressions of type 'decimal'" — limitação real do provider, não
        // do EF Core em geral (PostgreSQL, por exemplo, não teria esse
        // problema). Solução: buscar só a coluna decimal para memória e
        // somar em LINQ-to-Objects — mesmo padrão de cautela já usado no
        // resto deste arquivo para agregações em SQLite.
        var estimatedRevenue = (await _context.Quotes
            .Where(q => q.CompanyId == companyId && q.Status == QuoteStatus.Approved)
            .Select(q => q.Total)
            .ToListAsync(cancellationToken))
            .Sum();

        var inProgress = await _context.WorkOrders
            .CountAsync(w => w.CompanyId == companyId && w.Status == WorkOrderStatus.InProgress, cancellationToken);
        var completed = await _context.WorkOrders
            .CountAsync(w => w.CompanyId == companyId && w.Status == WorkOrderStatus.Completed, cancellationToken);

        // Mesma ideia de GetServiceRankingAsync (soma direto em
        // WorkOrderItems, sem agregação de coleção de navegação dentro de
        // Select) + a limitação do SQLite com Sum() em decimal explicada
        // acima: busca só a coluna decimal e soma em memória.
        var realizedRevenue = (await _context.WorkOrderItems
            .Where(i => i.WorkOrder.CompanyId == companyId && i.WorkOrder.Status == WorkOrderStatus.Completed)
            .Select(i => i.LineTotal)
            .ToListAsync(cancellationToken))
            .Sum();

        return new DashboardSummaryDto(
            totalQuotes, pendingQuotes, approvedQuotes, inProgress, completed, estimatedRevenue, realizedRevenue);
    }

    public async Task<IReadOnlyList<MonthlyAmount>> GetCompletedWorkOrderRevenueByMonthAsync(
        Guid companyId, DateTime sinceUtc, CancellationToken cancellationToken = default)
    {
        // UpdatedAt como "data de conclusão": não existe um campo CompletedAt
        // dedicado. Como UpdateStatusAsync bloqueia edição depois de
        // Completed, UpdatedAt reflete de forma confiável o momento exato da
        // conclusão. Se isso se mostrar insuficiente no futuro (ex.: preciso
        // de auditoria completa de quando cada status mudou), vale adicionar
        // um campo explícito.
        //
        // Mesma correção da GetSummaryAsync: busca direto em WorkOrderItems
        // (uma linha por item, sem Sum() de coleção dentro de Select) e
        // agrupa por mês em memória — igual já era feito para o mês, agora
        // também para não depender de subquery correlacionada.
        var rows = await _context.WorkOrderItems
            .Where(i => i.WorkOrder.CompanyId == companyId
                     && i.WorkOrder.Status == WorkOrderStatus.Completed
                     && i.WorkOrder.UpdatedAt >= sinceUtc)
            .Select(i => new { i.WorkOrder.UpdatedAt, i.LineTotal })
            .ToListAsync(cancellationToken);

        return GroupByMonth(rows.Where(r => r.UpdatedAt.HasValue), r => r.UpdatedAt!.Value, r => r.LineTotal);
    }

    public async Task<IReadOnlyList<MonthlyAmount>> GetApprovedQuoteRevenueByMonthAsync(
        Guid companyId, DateTime sinceUtc, CancellationToken cancellationToken = default)
    {
        var rows = await _context.Quotes
            .Where(q => q.CompanyId == companyId && q.Status == QuoteStatus.Approved && q.UpdatedAt >= sinceUtc)
            .Select(q => new { q.UpdatedAt, q.Total })
            .ToListAsync(cancellationToken);

        return GroupByMonth(rows.Where(r => r.UpdatedAt.HasValue), r => r.UpdatedAt!.Value, r => r.Total);
    }

    public async Task<QuoteStatusCounts> GetQuoteStatusCountsAsync(
        Guid companyId, DateTime? sinceUtc, CancellationToken cancellationToken = default)
    {
        var query = _context.Quotes.Where(q => q.CompanyId == companyId);

        if (sinceUtc.HasValue)
        {
            // Filtra por data de EMISSÃO (não de aprovação/rejeição): a
            // pergunta é "dos orçamentos emitidos nesse período, quantos
            // foram aceitos", não "quantos foram decididos nesse período".
            query = query.Where(q => q.IssueDate >= sinceUtc.Value);
        }

        var statuses = await query.Select(q => q.Status).ToListAsync(cancellationToken);

        return new QuoteStatusCounts(
            Draft: statuses.Count(s => s == QuoteStatus.Draft),
            Sent: statuses.Count(s => s == QuoteStatus.Sent),
            Approved: statuses.Count(s => s == QuoteStatus.Approved),
            Rejected: statuses.Count(s => s == QuoteStatus.Rejected),
            Expired: statuses.Count(s => s == QuoteStatus.Expired),
            Cancelled: statuses.Count(s => s == QuoteStatus.Cancelled));
    }

    public async Task<IReadOnlyList<ServiceRankingDto>> GetServiceRankingAsync(
        Guid companyId, int take, CancellationToken cancellationToken = default)
    {
        // Só itens de O.S. concluídas contam como "executado" — e só itens
        // ligados a um Service do catálogo entram no ranking (um item
        // avulso, sem ServiceId, não tem "serviço" para ranquear).
        var rows = await _context.WorkOrderItems
            .Where(i => i.WorkOrder.CompanyId == companyId
                     && i.WorkOrder.Status == WorkOrderStatus.Completed
                     && i.ServiceId != null)
            .Select(i => new { i.ServiceId, ServiceName = i.Service!.Name, i.Quantity, i.LineTotal })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(r => new { r.ServiceId, r.ServiceName })
            .Select(g => new ServiceRankingDto(
                g.Key.ServiceId!.Value,
                g.Key.ServiceName,
                g.Count(),
                g.Sum(x => x.Quantity),
                g.Sum(x => x.LineTotal)))
            .OrderByDescending(r => r.TotalRevenue)
            .Take(take)
            .ToList();
    }

    private static List<MonthlyAmount> GroupByMonth<T>(
        IEnumerable<T> rows, Func<T, DateTime> dateSelector, Func<T, decimal> amountSelector) =>
        rows
            .GroupBy(r =>
            {
                var date = dateSelector(r);
                return (date.Year, date.Month);
            })
            .Select(g => new MonthlyAmount(g.Key.Year, g.Key.Month, g.Sum(amountSelector)))
            .ToList();
}
