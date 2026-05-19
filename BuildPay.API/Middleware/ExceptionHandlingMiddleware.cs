using System.Net;
using System.Text.Json;
using FluentValidation;

namespace BuildPay.API.Middleware;

public sealed class ExceptionHandlingMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json";

            var payload = new
            {
                message = "Validation failed",
                errors = ex.Errors.Select(e => new { field = e.PropertyName, error = e.ErrorMessage }).ToList()
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
    }
}

