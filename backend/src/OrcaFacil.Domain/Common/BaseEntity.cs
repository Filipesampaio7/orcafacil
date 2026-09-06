namespace OrcaFacil.Domain.Common;

/// <summary>
/// Campos comuns a toda entidade do domínio. Entidades reais (Customer, Quote,
/// WorkOrder etc.) serão adicionadas na FASE 2, herdando desta classe.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
