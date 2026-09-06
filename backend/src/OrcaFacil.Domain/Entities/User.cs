using OrcaFacil.Domain.Common;
using OrcaFacil.Domain.Enums;

namespace OrcaFacil.Domain.Entities;

/// <summary>
/// PasswordHash fica pronto desde já (a coluna precisa existir na migration),
/// mas o hashing de verdade só é implementado na FASE 3 (Autenticação) — até
/// lá, nenhuma linha real deve ser inserida nesta tabela.
/// </summary>
public class User : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Owner;

    public ICollection<WorkOrder> AssignedWorkOrders { get; set; } = new List<WorkOrder>();
}
