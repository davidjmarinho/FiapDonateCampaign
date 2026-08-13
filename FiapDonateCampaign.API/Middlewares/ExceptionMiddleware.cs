using System.Net;
using System.Text.Json;
using FiapDonateCampaign.Domain.Exceptions;

namespace FiapDonateCampaign.API.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Regra de negócio violada: {Mensagem}", ex.Message);
            await EscreverResposta(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro não tratado");
            await EscreverResposta(context, HttpStatusCode.InternalServerError, "Ocorreu um erro interno. Tente novamente mais tarde.");
        }
    }

    private static async Task EscreverResposta(HttpContext context, HttpStatusCode statusCode, string mensagem)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var payload = JsonSerializer.Serialize(new { erro = mensagem });
        await context.Response.WriteAsync(payload);
    }
}