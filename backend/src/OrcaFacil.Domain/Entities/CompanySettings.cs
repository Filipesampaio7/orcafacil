using OrcaFacil.Domain.Common;

namespace OrcaFacil.Domain.Entities;

/// <summary>
/// Dados exibidos na tela de "Configurações" (FASE 10) e usados no cabeçalho
/// do PDF do orçamento (FASE 7): CNPJ, telefone, endereço, logo. Relação 1:1
/// com Company — cada empresa tem exatamente um registro de configurações.
/// </summary>
public class CompanySettings : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public string? Cnpj { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? LogoUrl { get; set; }

    public int DefaultQuoteValidityDays { get; set; } = 7;
    public string Currency { get; set; } = "BRL";
}
