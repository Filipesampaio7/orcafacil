using OrcaFacil.Application.DTOs.Customers;
using OrcaFacil.Application.Exceptions;
using OrcaFacil.Application.Interfaces;
using OrcaFacil.Domain.Entities;

namespace OrcaFacil.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public CustomerService(
        ICustomerRepository customerRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<CustomerResponseDto>> GetAllAsync(string? search, CancellationToken cancellationToken = default)
    {
        var customers = await _customerRepository.SearchAsync(CompanyId, search, cancellationToken);
        return customers.Select(ToResponseDto).ToList();
    }

    public async Task<CustomerResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await GetOwnedCustomerAsync(id, cancellationToken);
        return ToResponseDto(customer);
    }

    public async Task<CustomerResponseDto> CreateAsync(CreateCustomerDto request, CancellationToken cancellationToken = default)
    {
        var customer = new Customer
        {
            CompanyId = CompanyId,
            Name = request.Name,
            Phone = request.Phone,
            Email = request.Email,
            Document = request.Document,
            Address = request.Address,
            Notes = request.Notes,
        };

        await _customerRepository.AddAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponseDto(customer);
    }

    public async Task<CustomerResponseDto> UpdateAsync(Guid id, UpdateCustomerDto request, CancellationToken cancellationToken = default)
    {
        var customer = await GetOwnedCustomerAsync(id, cancellationToken);

        customer.Name = request.Name;
        customer.Phone = request.Phone;
        customer.Email = request.Email;
        customer.Document = request.Document;
        customer.Address = request.Address;
        customer.Notes = request.Notes;

        _customerRepository.Update(customer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponseDto(customer);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await GetOwnedCustomerAsync(id, cancellationToken);

        // Se o cliente já tiver orçamentos (FASE 6) ou ordens de serviço
        // (FASE 8), o banco recusa a exclusão — a FK está configurada como
        // Restrict (ver QuoteConfiguration/WorkOrderConfiguration). O EF Core
        // lança DbUpdateException nesse caso; não tratamos isso aqui ainda
        // porque nenhuma das duas tabelas tem dados até essas fases existirem.
        _customerRepository.Remove(customer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Customer> GetOwnedCustomerAsync(Guid id, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdForCompanyAsync(id, CompanyId, cancellationToken);
        return customer ?? throw new NotFoundException("Cliente não encontrado.");
    }

    private Guid CompanyId => _currentUserService.CompanyId
        ?? throw new InvalidCredentialsException();

    private static CustomerResponseDto ToResponseDto(Customer customer) => new(
        customer.Id,
        customer.Name,
        customer.Phone,
        customer.Email,
        customer.Document,
        customer.Address,
        customer.Notes,
        customer.CreatedAt);
}
