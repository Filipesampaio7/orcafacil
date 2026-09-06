using OrcaFacil.Domain.Enums;

namespace OrcaFacil.Application.DTOs.Quotes;

/// <summary>
/// Um item precisa vir de UM dos dois jeitos: ServiceId (item do catálogo —
/// Description/UnitPrice são preenchidos a partir do Service se omitidos) OU
/// Description + UnitPrice preenchidos à mão (produto/material avulso). O
/// validador (CreateQuoteItemValidator) garante essa regra antes de chegar
/// no Service da Application.
/// </summary>
public record QuoteItemInputDto(
    Guid? ServiceId,
    string? Description,
    decimal Quantity,
    decimal? UnitPrice,
    decimal DiscountAmount);

public record CreateQuoteDto(
    Guid CustomerId,
    DateTime ValidUntil,
    string? Notes,
    List<QuoteItemInputDto> Items);

public record UpdateQuoteDto(
    DateTime ValidUntil,
    string? Notes,
    List<QuoteItemInputDto> Items);

public record UpdateQuoteStatusDto(QuoteStatus Status);

public record QuoteItemResponseDto(
    Guid Id,
    Guid? ServiceId,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal DiscountAmount,
    decimal LineTotal);

/// <summary>Usado na listagem (GET /api/quotes) — sem os itens, para a resposta ficar leve.</summary>
public record QuoteSummaryDto(
    Guid Id,
    int Number,
    Guid CustomerId,
    string CustomerName,
    QuoteStatus Status,
    DateTime IssueDate,
    DateTime ValidUntil,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal Total);

/// <summary>Usado no detalhe (GET por id, criação, edição, mudança de status) — com os itens.</summary>
public record QuoteResponseDto(
    Guid Id,
    int Number,
    Guid CustomerId,
    string CustomerName,
    QuoteStatus Status,
    DateTime IssueDate,
    DateTime ValidUntil,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal Total,
    string? Notes,
    IReadOnlyList<QuoteItemResponseDto> Items);
