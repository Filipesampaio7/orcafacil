using OrcaFacil.Application.DTOs.Dashboard;
using OrcaFacil.Application.Interfaces;

namespace OrcaFacil.Application.Services;

/// <summary>
/// Função pura: dado o mês de referência e as linhas agregadas vindas do
/// repositório, sempre produz a mesma série de N meses — inclusive os meses
/// sem nenhum dado (com valor zero), porque um gráfico "comparado com meses
/// anteriores" fica errado se simplesmente pular um mês vazio.
/// </summary>
public static class MonthlyRevenueBuilder
{
    public static IReadOnlyList<MonthlyRevenueDto> Build(
        int months,
        DateTime referenceUtc,
        IReadOnlyList<MonthlyAmount> realized,
        IReadOnlyList<MonthlyAmount> estimated)
    {
        var result = new List<MonthlyRevenueDto>();
        var firstOfReferenceMonth = new DateTime(referenceUtc.Year, referenceUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        for (var offset = months - 1; offset >= 0; offset--)
        {
            var month = firstOfReferenceMonth.AddMonths(-offset);

            var realizedAmount = realized.FirstOrDefault(r => r.Year == month.Year && r.Month == month.Month)?.Amount ?? 0m;
            var estimatedAmount = estimated.FirstOrDefault(r => r.Year == month.Year && r.Month == month.Month)?.Amount ?? 0m;

            result.Add(new MonthlyRevenueDto(month.Year, month.Month, realizedAmount, estimatedAmount));
        }

        return result;
    }
}
