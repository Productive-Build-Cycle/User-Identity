using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Identity.API.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError("EXCEPTION! {ExType} - {ExMessage}", ex.GetType().Name, ex.Message);
        
            if(ex.InnerException is not null)
                _logger.LogError("EXCEPTION! {ExType} - {ExMessage}", ex.InnerException.GetType().Name, ex.InnerException.Message);


            var problemDetails = _environment.IsDevelopment()
                ? CreateDevelopmentProblemDetails(context, ex)
                : CreateProductionProblemDetails(context);

            context.Response.StatusCode = problemDetails.Status!.Value;

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(problemDetails)
            );
        }
    }

    private static ProblemDetails CreateDevelopmentProblemDetails(
        HttpContext context,
        Exception exception)
    {
        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = exception.GetType().Name,
            Detail = exception.Message
        };

        if (exception.InnerException is not null)
        {
            problem.Extensions["innerException"] = new
            {
                type = exception.InnerException.GetType().Name,
                message = exception.InnerException.Message
            };
        }

        return problem;
    }

    private static ProblemDetails CreateProductionProblemDetails(
        HttpContext context)
    {
        return new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Internal Server Error",
            Detail = "An unexpected error occurred. Please try again later."
        };
    }
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandlingMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
