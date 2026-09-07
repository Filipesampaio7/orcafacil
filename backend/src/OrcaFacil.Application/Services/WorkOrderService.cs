using OrcaFacil.Application.DTOs.WorkOrders;
using OrcaFacil.Application.Exceptions;
using OrcaFacil.Application.Interfaces;
using OrcaFacil.Domain.Entities;
using OrcaFacil.Domain.Enums;

namespace OrcaFacil.Application.Services;

public class WorkOrderService : IWorkOrderService
{
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly IQuoteRepository _quoteRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public WorkOrderService(
        IWorkOrderRepository workOrderRepository,
        IQuoteRepository quoteRepository,
        ICustomerRepository customerRepository,
        IServiceRepository serviceRepository,
        IUserRepository userRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _workOrderRepository = workOrderRepository;
        _quoteRepository = quoteRepository;
        _customerRepository = customerRepository;
        _serviceRepository = serviceRepository;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<WorkOrderSummaryDto>> GetAllAsync(
        WorkOrderStatus? status, Guid? customerId, Guid? assignedUserId, CancellationToken cancellationToken = default)
    {
        var workOrders = await _workOrderRepository.SearchAsync(CompanyId, status, customerId, assignedUserId, cancellationToken);
        return workOrders.Select(ToSummaryDto).ToList();
    }

    public async Task<WorkOrderResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var workOrder = await GetOwnedWorkOrderAsync(id, cancellationToken);
        return ToResponseDto(workOrder);
    }

    public async Task<WorkOrderResponseDto> CreateAsync(CreateWorkOrderDto request, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdForCompanyAsync(request.CustomerId, CompanyId, cancellationToken)
            ?? throw new NotFoundException("Cliente não encontrado.");

        var assignedUser = await ResolveAssignedUserAsync(request.AssignedUserId, cancellationToken);

        var workOrder = new WorkOrder
        {
            CompanyId = CompanyId,
            CustomerId = customer.Id,
            Customer = customer,
            AssignedUserId = assignedUser?.Id,
            AssignedUser = assignedUser,
            Status = WorkOrderStatus.Awaiting,
            ScheduledDate = request.ScheduledDate,
            Notes = request.Notes,
        };

        foreach (var itemDto in request.Items)
        {
            workOrder.Items.Add(await BuildItemAsync(itemDto, cancellationToken));
        }

        await _workOrderRepository.AddAsync(workOrder, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponseDto(workOrder);
    }

    public async Task<WorkOrderResponseDto> ConvertFromQuoteAsync(Guid quoteId, CancellationToken cancellationToken = default)
    {
        var quote = await _quoteRepository.GetByIdForCompanyAsync(quoteId, CompanyId, cancellationToken)
            ?? throw new NotFoundException("Orçamento não encontrado.");

        if (quote.Status != QuoteStatus.Approved)
        {
            throw new ConflictException("Só é possível transformar em ordem de serviço um orçamento aprovado.");
        }

        if (quote.WorkOrder is not null)
        {
            throw new ConflictException("Este orçamento já foi transformado em uma ordem de serviço.");
        }

        var workOrder = new WorkOrder
        {
            CompanyId = CompanyId,
            CustomerId = quote.CustomerId,
            Customer = quote.Customer,
            QuoteId = quote.Id,
            Quote = quote,
            Status = WorkOrderStatus.Awaiting,
            Notes = quote.Notes,
        };

        // Copia os itens do orçamento como "foto" — igual ao que QuoteItem faz
        // ao referenciar um Service. O desconto negociado no orçamento não é
        // carregado: a O.S. registra o que será executado, não a negociação
        // comercial que já foi fechada.
        foreach (var quoteItem in quote.Items)
        {
            var item = new WorkOrderItem
            {
                ServiceId = quoteItem.ServiceId,
                Description = quoteItem.Description,
                Quantity = quoteItem.Quantity,
                UnitPrice = quoteItem.UnitPrice,
            };
            item.Recalculate();
            workOrder.Items.Add(item);
        }

        await _workOrderRepository.AddAsync(workOrder, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponseDto(workOrder);
    }

    public async Task<WorkOrderResponseDto> UpdateAsync(Guid id, UpdateWorkOrderDto request, CancellationToken cancellationToken = default)
    {
        var workOrder = await GetOwnedWorkOrderAsync(id, cancellationToken);

        if (workOrder.Status is WorkOrderStatus.Completed or WorkOrderStatus.Cancelled)
        {
            throw new ConflictException("Não é possível editar uma ordem de serviço concluída ou cancelada.");
        }

        var assignedUser = await ResolveAssignedUserAsync(request.AssignedUserId, cancellationToken);

        workOrder.AssignedUserId = assignedUser?.Id;
        workOrder.AssignedUser = assignedUser;
        workOrder.ScheduledDate = request.ScheduledDate;
        workOrder.Notes = request.Notes;

        workOrder.Items.Clear();
        foreach (var itemDto in request.Items)
        {
            workOrder.Items.Add(await BuildItemAsync(itemDto, cancellationToken));
        }

        _workOrderRepository.Update(workOrder);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponseDto(workOrder);
    }

    public async Task<WorkOrderResponseDto> UpdateStatusAsync(Guid id, WorkOrderStatus newStatus, CancellationToken cancellationToken = default)
    {
        var workOrder = await GetOwnedWorkOrderAsync(id, cancellationToken);

        if (!WorkOrderStatusTransitions.CanTransition(workOrder.Status, newStatus))
        {
            throw new ConflictException($"Não é possível mudar a ordem de serviço de \"{workOrder.Status}\" para \"{newStatus}\".");
        }

        workOrder.Status = newStatus;
        _workOrderRepository.Update(workOrder);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponseDto(workOrder);
    }

    private async Task<WorkOrderItem> BuildItemAsync(WorkOrderItemInputDto dto, CancellationToken cancellationToken)
    {
        Service? service = null;
        if (dto.ServiceId.HasValue)
        {
            service = await _serviceRepository.GetByIdForCompanyAsync(dto.ServiceId.Value, CompanyId, cancellationToken)
                ?? throw new NotFoundException("Serviço não encontrado.");
        }

        var item = new WorkOrderItem
        {
            ServiceId = service?.Id,
            Description = !string.IsNullOrWhiteSpace(dto.Description) ? dto.Description! : service!.Name,
            Quantity = dto.Quantity,
            UnitPrice = dto.UnitPrice ?? service!.DefaultPrice,
        };
        item.Recalculate();
        return item;
    }

    /// <summary>Garante que o "responsável" informado é um usuário real da mesma empresa — nunca de outra.</summary>
    private async Task<User?> ResolveAssignedUserAsync(Guid? assignedUserId, CancellationToken cancellationToken)
    {
        if (!assignedUserId.HasValue)
        {
            return null;
        }

        var user = await _userRepository.GetByIdAsync(assignedUserId.Value, cancellationToken);
        if (user is null || user.CompanyId != CompanyId)
        {
            throw new NotFoundException("Usuário responsável não encontrado.");
        }

        return user;
    }

    private async Task<WorkOrder> GetOwnedWorkOrderAsync(Guid id, CancellationToken cancellationToken)
    {
        var workOrder = await _workOrderRepository.GetByIdForCompanyAsync(id, CompanyId, cancellationToken);
        return workOrder ?? throw new NotFoundException("Ordem de serviço não encontrada.");
    }

    private Guid CompanyId => _currentUserService.CompanyId
        ?? throw new InvalidCredentialsException();

    private static WorkOrderSummaryDto ToSummaryDto(WorkOrder workOrder) => new(
        workOrder.Id,
        workOrder.CustomerId,
        workOrder.Customer.Name,
        workOrder.AssignedUserId,
        workOrder.AssignedUser?.Name,
        workOrder.Status,
        workOrder.ScheduledDate,
        workOrder.Items.Sum(i => i.LineTotal));

    private static WorkOrderResponseDto ToResponseDto(WorkOrder workOrder) => new(
        workOrder.Id,
        workOrder.CustomerId,
        workOrder.Customer.Name,
        workOrder.QuoteId,
        workOrder.Quote?.Number,
        workOrder.AssignedUserId,
        workOrder.AssignedUser?.Name,
        workOrder.Status,
        workOrder.ScheduledDate,
        workOrder.Notes,
        workOrder.Items.Sum(i => i.LineTotal),
        workOrder.Items.Select(i => new WorkOrderItemResponseDto(
            i.Id, i.ServiceId, i.Description, i.Quantity, i.UnitPrice, i.LineTotal)).ToList());
}
