namespace OrcaFacil.Domain.Enums;

/// <summary>
/// Fica no Domain (não na Application) de propósito: "quais mudanças de
/// status fazem sentido" é regra de negócio, não detalhe de infraestrutura —
/// e assim dá para testar sem banco de dados nem HTTP envolvidos.
/// </summary>
public static class QuoteStatusTransitions
{
    private static readonly Dictionary<QuoteStatus, QuoteStatus[]> Allowed = new()
    {
        [QuoteStatus.Draft] = new[] { QuoteStatus.Sent, QuoteStatus.Cancelled },
        [QuoteStatus.Sent] = new[] { QuoteStatus.Approved, QuoteStatus.Rejected, QuoteStatus.Expired, QuoteStatus.Cancelled },
        [QuoteStatus.Approved] = new[] { QuoteStatus.Cancelled },
        [QuoteStatus.Rejected] = Array.Empty<QuoteStatus>(),
        [QuoteStatus.Expired] = Array.Empty<QuoteStatus>(),
        [QuoteStatus.Cancelled] = Array.Empty<QuoteStatus>(),
    };

    public static bool CanTransition(QuoteStatus from, QuoteStatus to) =>
        Allowed.TryGetValue(from, out var next) && next.Contains(to);
}
