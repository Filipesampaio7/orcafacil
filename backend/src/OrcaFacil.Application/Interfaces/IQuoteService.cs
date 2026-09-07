using OrcaFacil.Application.DTOs.Quotes;
using OrcaFacil.Domain.Enums;

namespace OrcaFacil.Application.Interfaces;

public interface IQuoteService
{
    Task<IReadOnlyList<QuoteSummaryDto>> GetAllAsync(
        QuoteStatus? status, Guid? customerId, CancellationToken cancellationToken = default);
    Task<QuoteResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<QuoteResponseDto> CreateAsync(CreateQuoteDto request, CancellationToken cancellationToken = default);
    Task<QuoteResponseDto> UpdateAsync(Guid id, UpdateQuoteDto request, CancellationToken cancellationToken = default);
    Task<QuoteResponseDto> UpdateStatusAsync(Guid id, QuoteStatus newStatus, CancellationToken cancellationToken = default);
    Task<byte[]> GeneratePdfAsync(Guid id, CancellationToken cancellationToken = default);
    Task<WhatsAppMessageDto> GenerateWhatsAppMessageAsync(Guid id, CancellationToken cancellationToken = default);
}
