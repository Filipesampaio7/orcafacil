using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using OrcaFacil.Application.DTOs.WorkOrders;
using OrcaFacil.Application.Interfaces;
using OrcaFacil.Domain.Enums;

namespace OrcaFacil.Api.Controllers;

[ApiController]
[Route("api/work-orders")]
public class WorkOrdersController : ControllerBase
{
    private readonly IWorkOrderService _workOrderService;
    private readonly IValidator<CreateWorkOrderDto> _createValidator;
    private readonly IValidator<UpdateWorkOrderDto> _updateValidator;
    private readonly IValidator<UpdateWorkOrderStatusDto> _updateStatusValidator;

    public WorkOrdersController(
        IWorkOrderService workOrderService,
        IValidator<CreateWorkOrderDto> createValidator,
        IValidator<UpdateWorkOrderDto> updateValidator,
        IValidator<UpdateWorkOrderStatusDto> updateStatusValidator)
    {
        _workOrderService = workOrderService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _updateStatusValidator = updateStatusValidator;
    }

    /// <summary>GET /api/work-orders?status=&amp;customerId=&amp;assignedUserId= — todos os filtros são opcionais.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<WorkOrderSummaryDto>>> GetAll(
        [FromQuery] WorkOrderStatus? status,
        [FromQuery] Guid? customerId,
        [FromQuery] Guid? assignedUserId,
        CancellationToken cancellationToken)
    {
        return Ok(await _workOrderService.GetAllAsync(status, customerId, assignedUserId, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WorkOrderResponseDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _workOrderService.GetByIdAsync(id, cancellationToken));
    }

    /// <summary>Cria uma ordem de serviço diretamente, sem passar por um orçamento.</summary>
    [HttpPost]
    public async Task<ActionResult<WorkOrderResponseDto>> Create(
        CreateWorkOrderDto request, CancellationToken cancellationToken)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            AddErrorsToModelState(validation);
            return ValidationProblem(ModelState);
        }

        var created = await _workOrderService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Transforma um orçamento aprovado em ordem de serviço, copiando os itens dele.</summary>
    [HttpPost("from-quote/{quoteId:guid}")]
    public async Task<ActionResult<WorkOrderResponseDto>> ConvertFromQuote(Guid quoteId, CancellationToken cancellationToken)
    {
        var created = await _workOrderService.ConvertFromQuoteAsync(quoteId, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Edita responsável, data prevista, observações e itens — só funciona fora de Concluído/Cancelado.</summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<WorkOrderResponseDto>> Update(
        Guid id, UpdateWorkOrderDto request, CancellationToken cancellationToken)
    {
        var validation = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            AddErrorsToModelState(validation);
            return ValidationProblem(ModelState);
        }

        return Ok(await _workOrderService.UpdateAsync(id, request, cancellationToken));
    }

    /// <summary>Muda o status (Aguardando → Agendado/Em andamento → Concluído/Cancelado) respeitando a máquina de estados.</summary>
    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<WorkOrderResponseDto>> UpdateStatus(
        Guid id, UpdateWorkOrderStatusDto request, CancellationToken cancellationToken)
    {
        var validation = await _updateStatusValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            AddErrorsToModelState(validation);
            return ValidationProblem(ModelState);
        }

        return Ok(await _workOrderService.UpdateStatusAsync(id, request.Status, cancellationToken));
    }

    /// <summary>Baixa a ordem de serviço em PDF.</summary>
    [HttpGet("{id:guid}/pdf")]
    public async Task<IActionResult> DownloadPdf(Guid id, CancellationToken cancellationToken)
    {
        var pdfBytes = await _workOrderService.GeneratePdfAsync(id, cancellationToken);
        return File(pdfBytes, "application/pdf", $"ordem-servico-{id}.pdf");
    }

    private void AddErrorsToModelState(FluentValidation.Results.ValidationResult validation)
    {
        foreach (var error in validation.Errors)
        {
            ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }
    }
}
