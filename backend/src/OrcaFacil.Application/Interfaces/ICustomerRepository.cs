using OrcaFacil.Domain.Entities;

namespace OrcaFacil.Application.Interfaces;

public interface ICustomerRepository : IRepository<Customer>
{
    /// <summary>
    /// Busca por Id + CompanyId ao mesmo tempo — nunca por Id sozinho. É o
    /// que impede que o usuário da Empresa A veja (ou edite) um cliente da
    /// Empresa B só adivinhando o Guid.
    /// </summary>
    Task<Customer?> GetByIdForCompanyAsync(Guid id, Guid companyId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Customer>> SearchAsync(Guid companyId, string? search, CancellationToken cancellationToken = default);
}
