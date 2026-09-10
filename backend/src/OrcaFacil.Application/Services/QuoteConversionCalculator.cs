namespace OrcaFacil.Application.Services;

/// <summary>
/// Função pura. A taxa considera só orçamentos já "decididos" (aprovados +
/// rejeitados) — orçamentos ainda em rascunho, enviados aguardando resposta,
/// expirados ou cancelados não entram no denominador, porque misturá-los
/// distorceria a leitura de "quantos orçamentos enviados o cliente aceita".
/// </summary>
public static class QuoteConversionCalculator
{
    public static decimal CalculateRate(int approvedCount, int rejectedCount)
    {
        var decided = approvedCount + rejectedCount;
        if (decided == 0)
        {
            return 0m;
        }

        return Math.Round((decimal)approvedCount / decided * 100m, 2);
    }
}
