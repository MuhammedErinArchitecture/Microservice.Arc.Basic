using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContentService.Api.Middleware;

public sealed class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHostEnvironment _env;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, IHostEnvironment env)
    {
        _next = next;
        _env = env;
    }

    public async Task Invoke(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (Exception ex)
        {
            await WriteProblemAsync(ctx, ex);
        }
    }

    private async Task WriteProblemAsync(HttpContext ctx, Exception ex)
    {
        var (status, title, detail) = MapToProblem(ex);
        var problem = new ProblemDetails
        {
            Title = title,
            Detail = detail,
            Status = status,
            Type = status is null ? null : $"https://httpstatuses.com/{status}",
            Instance = ctx.Request.Path
        };

        var traceId = ctx.TraceIdentifier;
        problem.Extensions["traceId"] = traceId;

        if (_env.IsDevelopment())
        {
            problem.Extensions["exception"] = new
            {
                type = ex.GetType().FullName,
                message = ex.Message
            };
        }

        ctx.Response.ContentType = "application/problem+json";
        ctx.Response.StatusCode = status ?? (int)HttpStatusCode.InternalServerError;
        var json = JsonSerializer.Serialize(problem);
        await ctx.Response.WriteAsync(json);
    }

    private static (int? Status, string Title, string Detail) MapToProblem(Exception ex)
    {
        if (ex.GetType().Name is "AuthorNotFoundException")
        {
            return (422, "Author not found", ex.Message);
        }

        if (ex.GetType().Name is "SlugConflictException")
        {
            return (409, "Slug conflict", ex.Message);
        }

        if (ex.GetType().Name is "EmailConflictException")
        {
            return (409, "Email conflict", ex.Message);
        }


        if (ex is DbUpdateException dbEx && dbEx.InnerException is not null)
        {
            var inner = dbEx.InnerException;
            var sqlState = inner.GetType().GetProperty("SqlState")?.GetValue(inner)?.ToString();
            var constraint = inner.GetType().GetProperty("ConstraintName")?.GetValue(inner)?.ToString();

            if (sqlState == "23505")
            {
                if (string.Equals(constraint, "IX_contents_slug", StringComparison.Ordinal))
                    return (409, "Slug conflict", "A content with the same slug already exists.");

                if (string.Equals(constraint, "IX_users_email", StringComparison.Ordinal))
                    return (409, "Email conflict", "A user with the same email already exists.");

                return (409, "Unique constraint violation", "A resource with the same unique value exists.");
            }
        }

        if (ex is TaskCanceledException || ex.GetType().Name.Contains("TimeoutRejectedException"))
            return (503, "Upstream timeout", "A dependent service did not respond in time.");
        if (ex is HttpRequestException)
            return (503, "Upstream unavailable", ex.Message);

        return (500, "Internal server error", "An unexpected error occurred.");
    }
}
