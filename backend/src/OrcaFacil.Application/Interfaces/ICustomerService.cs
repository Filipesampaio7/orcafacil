using OrcaFacil.Application.DTOs.Customers;

namespace OrcaFacil.Application.Interfaces;

public interface ICustomerService
{
    Task<IReadOnlyList<CustomerResponseDto>> GetAllAsync(string? search, CancellationToken cancellationToken = default);
    Task<CustomerResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CustomerResponseDto> CreateAsync(CreateCustomerDto request, CancellationToken cancellationToken = default);
    Task<CustomerResponseDto> UpdateAsync(Guid id, UpdateCustomerDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
