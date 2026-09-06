using System.Net;
using System.Text.Json;
using OrcaFacil.Application.Exceptions;

namespace OrcaFacil.Api.Middleware;

/// <summary>
/// Conceito: "middleware" no ASP.NET Core é uma peça que envolve o pipeline
/// inteiro da requisição. Colocando este logo no início (veja Program.cs),
/// qualquer exceção lançada por um controller ou service, em qualquer lugar
/// abaixo, passa por aqui antes de virar resposta HTTP — os controllers não
/// precisam de try/catch repetido em cada action.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Erro não tratado ao processar {Path}", context.Request.Path);
            await WriteErrorResponseAsync(context, exception);
        }
    }

    private static Task WriteErrorResponseAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            ConflictException => (HttpStatusCode.Conflict, exception.Message),
            InvalidCredentialsException => (HttpStatusCode.Unauthorized, exception.Message),
            // Nunca vazamos a mensagem de exceções desconhecidas para o
            // cliente — poderia expor detalhes internos (stack, nomes de
            // tabela). O detalhe completo já foi para o log acima.
            _ => (HttpStatusCode.InternalServerError, "Ocorreu um erro inesperado."),
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var payload = JsonSerializer.Serialize(new { error = message });
        return context.Response.WriteAsync(payload);
    }
}
