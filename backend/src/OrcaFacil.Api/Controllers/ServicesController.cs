using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using OrcaFacil.Application.DTOs.Services;
using OrcaFacil.Application.Interfaces;
using OrcaFacil.Domain.Enums;

namespace OrcaFacil.Api.Controllers;

[ApiController]
[Route("api/services")]
public class ServicesController : ControllerBase
{
    private readonly IServiceCatalogService _serviceCatalogService;
    private readonly IValidator<CreateServiceDto> _createValidator;
    private readonly IValidator<UpdateServiceDto> _updateValidator;

    public ServicesController(
        IServiceCatalogService serviceCatalogService,
        IValidator<CreateServiceDto> createValidator,
        IValidator<UpdateServiceDto> updateValidator)
    {
        _serviceCatalogService = serviceCatalogService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    /// <summary>GET /api/services?search=&amp;category=&amp;status= — todos os filtros são opcionais.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ServiceResponseDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? category,
        [FromQuery] ServiceStatus? status,
        CancellationToken cancellationToken)
    {
        return Ok(await _serviceCatalogService.GetAllAsync(search, category, status, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ServiceResponseDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _serviceCatalogService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<ServiceResponseDto>> Create(
        CreateServiceDto request, CancellationToken cancellationToken)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            AddErrorsToModelState(validation);
            return ValidationProblem(ModelState);
        }

        var created = await _serviceCatalogService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ServiceResponseDto>> Update(
        Guid id, UpdateServiceDto request, CancellationToken cancellationToken)
    {
        var validation = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            AddErrorsToModelState(validation);
            return ValidationProblem(ModelState);
        }

        return Ok(await _serviceCatalogService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _serviceCatalogService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    private void AddErrorsToModelState(FluentValidation.Results.ValidationResult validation)
    {
        foreach (var error in validation.Errors)
        {
            ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }
    }
}
