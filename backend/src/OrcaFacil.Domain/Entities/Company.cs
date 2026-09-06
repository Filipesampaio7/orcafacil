using OrcaFacil.Domain.Common;

namespace OrcaFacil.Domain.Entities;

/// <summary>
/// Raiz do "tenant": tudo no sistema pertence a uma Company. Mantida enxuta
/// de propósito — os dados editáveis (CNPJ, telefone, endereço, logo) ficam
/// em <see cref="CompanySettings"/>, para separar "quem é a empresa no
/// sistema" de "como a empresa se configura".
/// </summary>
public class Company : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public CompanySettings? Settings { get; set; }
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Customer> Customers { get; set; } = new List<Customer>();
    public ICollection<Service> Services { get; set; } = new List<Service>();
    public ICollection<Quote> Quotes { get; set; } = new List<Quote>();
    public ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
}
