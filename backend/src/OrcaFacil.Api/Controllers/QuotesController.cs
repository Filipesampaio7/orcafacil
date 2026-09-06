using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using OrcaFacil.Application.DTOs.Quotes;
using OrcaFacil.Application.Interfaces;
using OrcaFacil.Domain.Enums;

namespace OrcaFacil.Api.Controllers;

[ApiController]
[Route("api/quotes")]
public class QuotesController : ControllerBase
{
    private readonly IQuoteService _quoteService;
    private readonly IValidator<CreateQuoteDto> _createValidator;
    private readonly IValidator<UpdateQuoteDto> _updateValidator;
    private readonly IValidator<UpdateQuoteStatusDto> _updateStatusValidator;

    public QuotesController(
        IQuoteService quoteService,
        IValidator<CreateQuoteDto> createValidator,
        IValidator<UpdateQuoteDto> updateValidator,
        IValidator<UpdateQuoteStatusDto> updateStatusValidator)
    {
        _quoteService = quoteService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _updateStatusValidator = updateStatusValidator;
    }

    /// <summary>GET /api/quotes?status=&amp;customerId= — ambos os filtros são opcionais.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<QuoteSummaryDto>>> GetAll(
        [FromQuery] QuoteStatus? status, [FromQuery] Guid? customerId, CancellationToken cancellationToken)
    {
        return Ok(await _quoteService.GetAllAsync(status, customerId, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<QuoteResponseDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _quoteService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<QuoteResponseDto>> Create(CreateQuoteDto request, CancellationToken cancellationToken)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            AddErrorsToModelState(validation);
            return ValidationProblem(ModelState);
        }

        var created = await _quoteService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Substitui itens, observações e validade — só funciona enquanto o orçamento está em rascunho.</summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<QuoteResponseDto>> Update(
        Guid id, UpdateQuoteDto request, CancellationToken cancellationToken)
    {
        var validation = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            AddErrorsToModelState(validation);
            return ValidationProblem(ModelState);
        }

        return Ok(await _quoteService.UpdateAsync(id, request, cancellationToken));
    }

    /// <summary>Muda o status (Rascunho → Enviado → Aprovado/Recusado/Expirado/Cancelado) respeitando a máquina de estados.</summary>
    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<QuoteResponseDto>> UpdateStatus(
        Guid id, UpdateQuoteStatusDto request, CancellationToken cancellationToken)
    {
        var validation = await _updateStatusValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            AddErrorsToModelState(validation);
            return ValidationProblem(ModelState);
        }

        return Ok(await _quoteService.UpdateStatusAsync(id, request.Status, cancellationToken));
    }

    private void AddErrorsToModelState(FluentValidation.Results.ValidationResult validation)
    {
        foreach (var error in validation.Errors)
        {
            ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }
    }
}
