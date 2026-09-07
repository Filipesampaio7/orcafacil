using System.Globalization;
using OrcaFacil.Application.DTOs.Quotes;

namespace OrcaFacil.Application.Services;

/// <summary>
/// Função pura de propósito: dado texto e números, sempre produz o mesmo
/// resultado. Isso permite testar a formatação da mensagem sem precisar de
/// banco, HTTP ou qualquer outra dependência — só os valores de entrada.
/// </summary>
public static class WhatsAppMessageBuilder
{
    // Formatação de número/data explícita em pt-BR — sem isso, a interpolação
    // ({total:N2}) usaria a cultura do sistema operacional do servidor onde a
    // API roda, que pode não ser pt-BR (gerando "350.50" em vez de "350,50").
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    public static WhatsAppMessageDto Build(
        string customerName, string? customerPhone, int quoteNumber, decimal total, DateTime validUntil)
    {
        var number = quoteNumber.ToString("D4", PtBr);
        var formattedTotal = total.ToString("N2", PtBr);
        var formattedDate = validUntil.ToString("dd/MM/yyyy", PtBr);

        var message =
            $"Olá, {customerName}! Seu orçamento #{number} no valor de R$ {formattedTotal} está disponível. " +
            $"Válido até {formattedDate}.";

        var link = BuildWhatsAppLink(customerPhone, message);

        return new WhatsAppMessageDto(message, link);
    }

    private static string? BuildWhatsAppLink(string? phone, string message)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return null;
        }

        // Simplificação de MVP: só remove tudo que não é dígito. Não valida
        // nem completa DDI/DDD — se o número cadastrado não incluir o código
        // do país (ex.: 55 para o Brasil), o link "wa.me" pode não abrir a
        // conversa certa. Uma normalização de telefone de verdade fica para
        // quando isso se mostrar um problema real.
        var digitsOnly = new string(phone.Where(char.IsDigit).ToArray());
        if (digitsOnly.Length == 0)
        {
            return null;
        }

        var encodedMessage = Uri.EscapeDataString(message);
        return $"https://wa.me/{digitsOnly}?text={encodedMessage}";
    }
}
