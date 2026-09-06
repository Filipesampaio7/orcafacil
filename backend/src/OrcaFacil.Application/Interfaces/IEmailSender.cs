namespace OrcaFacil.Application.Interfaces;

/// <summary>
/// Contrato preparado para quando o fluxo de "esqueci minha senha" for
/// implementado (fora do escopo desta FASE 3, por pedido explícito). Nenhuma
/// implementação existe ainda — quando for a hora, um EmailSender concreto
/// (SMTP, SendGrid etc.) entra em OrcaFacil.Infrastructure.
/// </summary>
public interface IEmailSender
{
    Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
}
