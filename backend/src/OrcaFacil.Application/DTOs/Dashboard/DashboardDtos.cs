namespace OrcaFacil.Application.DTOs.Dashboard;

/// <summary>Visão geral — cobre os números originais pedidos para o dashboard (orçamentos, O.S., faturamento).</summary>
public record DashboardSummaryDto(
    int TotalQuotes,
    int PendingQuotes,
    int ApprovedQuotes,
    int WorkOrdersInProgress,
    int WorkOrdersCompleted,
    decimal EstimatedRevenue,
    decimal RealizedRevenue);

/// <summary>
/// Um ponto na série de faturamento mensal. RealizedRevenue vem de O.S.
/// concluídas naquele mês; EstimatedRevenue vem de orçamentos aprovados
/// naquele mês (podem nunca virar O.S., ou virar em outro mês).
/// </summary>
public record MonthlyRevenueDto(int Year, int Month, decimal RealizedRevenue, decimal EstimatedRevenue);

/// <summary>Taxa de conversão de orçamentos — contagem por status + percentual de aprovação entre os já decididos.</summary>
public record QuoteConversionDto(
    int TotalQuotes,
    int DraftCount,
    int SentCount,
    int ApprovedCount,
    int RejectedCount,
    int ExpiredCount,
    int CancelledCount,
    decimal ConversionRate);

/// <summary>Um serviço do catálogo e seu desempenho em O.S. concluídas.</summary>
public record ServiceRankingDto(
    Guid ServiceId,
    string ServiceName,
    int ExecutionCount,
    decimal TotalQuantity,
    decimal TotalRevenue);
