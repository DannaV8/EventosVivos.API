using System.Text.Json;
using EventosVivos.Application.Common.Exceptions;
using EventosVivos.Domain.Exceptions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.API.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHostEnvironment _env;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        IHostEnvironment env,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next   = next;
        _env    = env;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private Task HandleAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";

        var (status, error, message, detail) = ex switch
        {
            NotFoundException nfe =>
                (404, "NOT_FOUND", nfe.Message, (object?)null),

            ValidationException ve =>
                (400, "VALIDATION_ERROR", "La solicitud contiene errores de validación.",
                 ve.Errors
                   .GroupBy(e => e.PropertyName)
                   .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())),

            CapacityExceededException cee =>
                (409, cee.Code, "No hay suficientes entradas disponibles.", (object?)cee.Message),

            VenueConflictException vce =>
                (409, vce.Code, vce.Message, null),

            InvalidReservationStateException irse =>
                (422, irse.Code, irse.Message, null),

            InvalidCredentialsException ice =>
                (401, ice.Code, ice.Message, null),

            DomainException de =>
                (400, de.Code, de.Message, null),

            InvalidOperationException ioe =>
                (400, "INVALID_OPERATION", ioe.Message, null),

            DbUpdateConcurrencyException =>
                (409, "CONCURRENCY_CONFLICT",
                 "El recurso fue modificado por otra operación. Intente nuevamente.", null),

            _ =>
                (500, "INTERNAL_ERROR", "Error interno del servidor.",
                 _env.IsDevelopment() ? (object?)ex.ToString() : null)
        };

        // Loguear con nivel apropiado según la gravedad
        if (status >= 500)
            _logger.LogError(ex, "Error {Status} [{Code}] en {Method} {Path}",
                status, error, context.Request.Method, context.Request.Path);
        else if (status >= 400)
            _logger.LogWarning("Error {Status} [{Code}] en {Method} {Path}: {Message}",
                status, error, context.Request.Method, context.Request.Path, message);

        context.Response.StatusCode = status;

        var body = new ErrorResponse(error, message, detail);
        var json = JsonSerializer.Serialize(body, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });

        return context.Response.WriteAsync(json);
    }

    private sealed record ErrorResponse(string Error, string Message, object? Detail);
}
