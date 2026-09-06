using OrcaFacil.Application.DTOs.Quotes;
using OrcaFacil.Application.Exceptions;
using OrcaFacil.Application.Interfaces;
using OrcaFacil.Domain.Entities;
using OrcaFacil.Domain.Enums;

namespace OrcaFacil.Application.Services;

public class QuoteService : IQuoteService
{
    private readonly IQuoteRepository _quoteRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public QuoteService(
        IQuoteRepository quoteRepository,
        ICustomerRepository customerRepository,
        IServiceRepository serviceRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _quoteRepository = quoteRepository;
        _customerRepository = customerRepository;
        _serviceRepository = serviceRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<QuoteSummaryDto>> GetAllAsync(
        QuoteStatus? status, Guid? customerId, CancellationToken cancellationToken = default)
    {
        var quotes = await _quoteRepository.SearchAsync(CompanyId, status, customerId, cancellationToken);
        return quotes.Select(ToSummaryDto).ToList();
    }

    public async Task<QuoteResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var quote = await GetOwnedQuoteAsync(id, cancellationToken);
        return ToResponseDto(quote);
    }

    public async Task<QuoteResponseDto> CreateAsync(CreateQuoteDto request, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdForCompanyAsync(request.CustomerId, CompanyId, cancellationToken)
            ?? throw new NotFoundException("Cliente não encontrado.");

        var quote = new Quote
        {
            CompanyId = CompanyId,
            CustomerId = customer.Id,
            Customer = customer,
            Number = await _quoteRepository.GetNextNumberAsync(CompanyId, cancellationToken),
            Status = QuoteStatus.Draft,
            IssueDate = DateTime.UtcNow,
            ValidUntil = request.ValidUntil,
            Notes = request.Notes,
        };

        foreach (var itemDto in request.Items)
        {
            quote.Items.Add(await BuildItemAsync(itemDto, cancellationToken));
        }

        quote.RecalculateTotals();

        await _quoteRepository.AddAsync(quote, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponseDto(quote);
    }

    public async Task<QuoteResponseDto> UpdateAsync(Guid id, UpdateQuoteDto request, CancellationToken cancellationToken = default)
    {
        var quote = await GetOwnedQuoteAsync(id, cancellationToken);

        // Um orçamento já enviado é, na prática, um documento que o cliente
        // já pode ter visto — editar os valores dele silenciosamente depois
        // quebraria a confiança no que foi comunicado. Só rascunho é editável.
        if (quote.Status != QuoteStatus.Draft)
        {
            throw new ConflictException("Só é possível editar um orçamento enquanto ele está em rascunho.");
        }

        quote.ValidUntil = request.ValidUntil;
        quote.Notes = request.Notes;

        quote.Items.Clear();
        foreach (var itemDto in request.Items)
        {
            quote.Items.Add(await BuildItemAsync(itemDto, cancellationToken));
        }

        quote.RecalculateTotals();

        _quoteRepository.Update(quote);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponseDto(quote);
    }

    public async Task<QuoteResponseDto> UpdateStatusAsync(Guid id, QuoteStatus newStatus, CancellationToken cancellationToken = default)
    {
        var quote = await GetOwnedQuoteAsync(id, cancellationToken);

        if (!QuoteStatusTransitions.CanTransition(quote.Status, newStatus))
        {
            throw new ConflictException($"Não é possível mudar o orçamento de \"{quote.Status}\" para \"{newStatus}\".");
        }

        quote.Status = newStatus;
        _quoteRepository.Update(quote);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponseDto(quote);
    }

    /// <summary>
    /// Resolve um item a partir de ServiceId (copiando nome/preço do catálogo
    /// como "foto" no momento da criação) ou dos dados avulsos informados. A
    /// invariante "ServiceId OU (Description + UnitPrice)" já foi garantida
    /// pelo QuoteItemInputValidator antes de chegar aqui.
    /// </summary>
    private async Task<QuoteItem> BuildItemAsync(QuoteItemInputDto dto, CancellationToken cancellationToken)
    {
        Service? service = null;
        if (dto.ServiceId.HasValue)
        {
            service = await _serviceRepository.GetByIdForCompanyAsync(dto.ServiceId.Value, CompanyId, cancellationToken)
                ?? throw new NotFoundException("Serviço não encontrado.");
        }

        return new QuoteItem
        {
            ServiceId = service?.Id,
            Description = !string.IsNullOrWhiteSpace(dto.Description) ? dto.Description! : service!.Name,
            Quantity = dto.Quantity,
            UnitPrice = dto.UnitPrice ?? service!.DefaultPrice,
            DiscountAmount = dto.DiscountAmount,
        };
    }

    private async Task<Quote> GetOwnedQuoteAsync(Guid id, CancellationToken cancellationToken)
    {
        var quote = await _quoteRepository.GetByIdForCompanyAsync(id, CompanyId, cancellationToken);
        return quote ?? throw new NotFoundException("Orçamento não encontrado.");
    }

    private Guid CompanyId => _currentUserService.CompanyId
        ?? throw new InvalidCredentialsException();

    private static QuoteSummaryDto ToSummaryDto(Quote quote) => new(
        quote.Id, quote.Number, quote.CustomerId, quote.Customer.Name,
        quote.Status, quote.IssueDate, quote.ValidUntil, quote.Subtotal, quote.DiscountAmount, quote.Total);

    private static QuoteResponseDto ToResponseDto(Quote quote) => new(
        quote.Id, quote.Number, quote.CustomerId, quote.Customer.Name,
        quote.Status, quote.IssueDate, quote.ValidUntil, quote.Subtotal, quote.DiscountAmount, quote.Total, quote.Notes,
        quote.Items.Select(i => new QuoteItemResponseDto(
            i.Id, i.ServiceId, i.Description, i.Quantity, i.UnitPrice, i.DiscountAmount, i.LineTotal)).ToList());
}
