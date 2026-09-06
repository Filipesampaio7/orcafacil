using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrcaFacil.Infrastructure.Data;

namespace OrcaFacil.Api.Controllers;

/// <summary>
/// Endpoint mínimo para validar que a API sobe e que a conexão com o banco
/// está correta. [AllowAnonymous] é necessário porque, a partir da FASE 3,
/// todo controller exige autenticação por padrão — sem isso, até uma checagem
/// de infraestrutura pediria login.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public HealthController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);

        return Ok(new
        {
            status = "ok",
            database = canConnect ? "connected" : "unreachable",
        });
    }
}
