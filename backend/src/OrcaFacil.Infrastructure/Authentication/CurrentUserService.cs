using Microsoft.AspNetCore.Http;
using OrcaFacil.Application.Interfaces;

namespace OrcaFacil.Infrastructure.Authentication;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private System.Security.Claims.ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public Guid? UserId => Guid.TryParse(User?.FindFirst("sub")?.Value, out var id) ? id : null;

    public Guid? CompanyId => Guid.TryParse(User?.FindFirst("companyId")?.Value, out var id) ? id : null;

    public string? Email => User?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
        ?? User?.FindFirst("email")?.Value;

    public string? Name => User?.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

    public string? CompanyName => User?.FindFirst("companyName")?.Value;
}
