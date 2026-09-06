using OrcaFacil.Application.DTOs.Services;
using OrcaFacil.Application.Exceptions;
using OrcaFacil.Application.Interfaces;
using OrcaFacil.Domain.Entities;
using OrcaFacil.Domain.Enums;

namespace OrcaFacil.Application.Services;

public class ServiceCatalogService : IServiceCatalogService
{
    private readonly IServiceRepository _serviceRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public ServiceCatalogService(
        IServiceRepository serviceRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _serviceRepository = serviceRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<ServiceResponseDto>> GetAllAsync(
        string? search, string? category, ServiceStatus? status, CancellationToken cancellationToken = default)
    {
        var services = await _serviceRepository.SearchAsync(CompanyId, search, category, status, cancellationToken);
        return services.Select(ToResponseDto).ToList();
    }

    public async Task<ServiceResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var service = await GetOwnedServiceAsync(id, cancellationToken);
        return ToResponseDto(service);
    }

    public async Task<ServiceResponseDto> CreateAsync(CreateServiceDto request, CancellationToken cancellationToken = default)
    {
        var service = new Service
        {
            CompanyId = CompanyId,
            Name = request.Name,
            Description = request.Description,
            DefaultPrice = request.DefaultPrice,
            EstimatedDurationMinutes = request.EstimatedDurationMinutes,
            Category = request.Category,
            Status = ServiceStatus.Active,
        };

        await _serviceRepository.AddAsync(service, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponseDto(service);
    }

    public async Task<ServiceResponseDto> UpdateAsync(Guid id, UpdateServiceDto request, CancellationToken cancellationToken = default)
    {
        var service = await GetOwnedServiceAsync(id, cancellationToken);

        service.Name = request.Name;
        service.Description = request.Description;
        service.DefaultPrice = request.DefaultPrice;
        service.EstimatedDurationMinutes = request.EstimatedDurationMinutes;
        service.Category = request.Category;
        service.Status = request.Status;

        _serviceRepository.Update(service);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponseDto(service);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var service = await GetOwnedServiceAsync(id, cancellationToken);

        // Igual ao Customer: QuoteItem/WorkOrderItem apontam para Service com
        // DeleteBehavior.SetNull, então excluir um serviço nunca quebra um
        // orçamento antigo — o item só perde a referência ao catálogo,
        // mantendo a descrição/preço que já tinha sido copiados.
        _serviceRepository.Remove(service);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Service> GetOwnedServiceAsync(Guid id, CancellationToken cancellationToken)
    {
        var service = await _serviceRepository.GetByIdForCompanyAsync(id, CompanyId, cancellationToken);
        return service ?? throw new NotFoundException("Serviço não encontrado.");
    }

    private Guid CompanyId => _currentUserService.CompanyId
        ?? throw new InvalidCredentialsException();

    private static ServiceResponseDto ToResponseDto(Service service) => new(
        service.Id,
        service.Name,
        service.Description,
        service.DefaultPrice,
        service.EstimatedDurationMinutes,
        service.Category,
        service.Status,
        service.CreatedAt);
}
