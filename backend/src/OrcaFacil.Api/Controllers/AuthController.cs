using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrcaFacil.Application.DTOs.Auth;
using OrcaFacil.Application.Interfaces;

namespace OrcaFacil.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<RegisterRequestDto> _registerValidator;
    private readonly IValidator<LoginRequestDto> _loginValidator;

    public AuthController(
        IAuthService authService,
        ICurrentUserService currentUserService,
        IValidator<RegisterRequestDto> registerValidator,
        IValidator<LoginRequestDto> loginValidator)
    {
        _authService = authService;
        _currentUserService = currentUserService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(
        RegisterRequestDto request, CancellationToken cancellationToken)
    {
        var validation = await _registerValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            AddValidationErrorsToModelState(validation);
            return ValidationProblem(ModelState);
        }

        var result = await _authService.RegisterAsync(request, cancellationToken);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(
        LoginRequestDto request, CancellationToken cancellationToken)
    {
        var validation = await _loginValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            AddValidationErrorsToModelState(validation);
            return ValidationProblem(ModelState);
        }

        var result = await _authService.LoginAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// JWT é stateless — não existe uma sessão no servidor para "derrubar".
    /// Este endpoint existe para o frontend ter um lugar único e protegido
    /// para chamar antes de descartar o token localmente. Se no futuro for
    /// necessário invalidar um token antes do seu vencimento (ex.: usuário
    /// trocou a senha), a solução é uma blacklist de tokens ou refresh
    /// tokens — nenhuma das duas está no escopo desta FASE 3.
    /// </summary>
    [HttpPost("logout")]
    public IActionResult Logout() => NoContent();

    [HttpGet("me")]
    public ActionResult<CurrentUserDto> Me()
    {
        if (_currentUserService.UserId is not { } userId ||
            _currentUserService.CompanyId is not { } companyId)
        {
            return Unauthorized();
        }

        var currentUser = new CurrentUserDto(
            userId,
            _currentUserService.Name ?? string.Empty,
            _currentUserService.Email ?? string.Empty,
            User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? string.Empty,
            companyId,
            _currentUserService.CompanyName ?? string.Empty);

        return Ok(currentUser);
    }

    private void AddValidationErrorsToModelState(FluentValidation.Results.ValidationResult validation)
    {
        foreach (var error in validation.Errors)
        {
            ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }
    }
}
