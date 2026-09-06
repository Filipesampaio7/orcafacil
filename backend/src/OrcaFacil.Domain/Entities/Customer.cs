using OrcaFacil.Domain.Common;

namespace OrcaFacil.Domain.Entities;

public class Customer : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Document { get; set; } // CPF/CNPJ, opcional
    public string? Address { get; set; }
    public string? Notes { get; set; }

    // CreatedAt (herdado de BaseEntity) já cobre "data de cadastro".

    public ICollection<Quote> Quotes { get; set; } = new List<Quote>();
    public ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
}
