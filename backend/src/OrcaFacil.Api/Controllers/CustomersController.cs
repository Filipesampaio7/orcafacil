using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using OrcaFacil.Application.DTOs.Customers;
using OrcaFacil.Application.Interfaces;

namespace OrcaFacil.Api.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly IValidator<CreateCustomerDto> _createValidator;
    private readonly IValidator<UpdateCustomerDto> _updateValidator;

    public CustomersController(
        ICustomerService customerService,
        IValidator<CreateCustomerDto> createValidator,
        IValidator<UpdateCustomerDto> updateValidator)
    {
        _customerService = customerService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    /// <summary>GET /api/customers?search=termo — pesquisa por nome, telefone, e-mail ou documento.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CustomerResponseDto>>> GetAll(
        [FromQuery] string? search, CancellationToken cancellationToken)
    {
        return Ok(await _customerService.GetAllAsync(search, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CustomerResponseDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _customerService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<CustomerResponseDto>> Create(
        CreateCustomerDto request, CancellationToken cancellationToken)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            AddErrorsToModelState(validation);
            return ValidationProblem(ModelState);
        }

        var created = await _customerService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CustomerResponseDto>> Update(
        Guid id, UpdateCustomerDto request, CancellationToken cancellationToken)
    {
        var validation = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            AddErrorsToModelState(validation);
            return ValidationProblem(ModelState);
        }

        return Ok(await _customerService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _customerService.DeleteAsync(id, cancellationToken);
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
