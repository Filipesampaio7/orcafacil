namespace OrcaFacil.Application.DTOs.Quotes;

/// <summary>
/// WhatsAppLink é nulo quando o cliente não tem telefone cadastrado — nesse
/// caso o frontend só oferece "copiar mensagem", não "abrir no WhatsApp".
/// Nenhuma integração oficial com a API do WhatsApp foi feita aqui, por
/// pedido explícito — isto é só um link "wa.me" com o texto pré-preenchido,
/// que abre o WhatsApp do próprio usuário.
/// </summary>
public record WhatsAppMessageDto(string Message, string? WhatsAppLink);
