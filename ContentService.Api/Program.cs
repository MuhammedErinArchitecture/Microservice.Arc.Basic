using BuildingBlocks.Abstractions.CQRS;
using ContentService.Api.Middleware;
using ContentService.Application.Contents;
using ContentService.Application.CQRS;
using ContentService.Domain.Services;
using ContentService.Infrastructure;
using FluentValidation;
using FluentValidation.AspNetCore;

namespace ContentService.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddHealthChecks();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddMemoryCache(); 

            builder.Services.AddControllers();
            builder.Services.AddFluentValidationAutoValidation();
            builder.Services.AddValidatorsFromAssemblyContaining<Validation.ContentCreateRequestValidator>();

            builder.Services.AddContentServiceInfrastructure(builder.Configuration);
            builder.Services.AddSingleton<IQueryBus, QueryBus>();
            builder.Services.AddScoped<IContentDomainService, ContentDomainService>();

            var app = builder.Build();
            app.MapGet("/healthz", () => Results.Ok(new { status = "ok", service = "content-api" }));

            app.MapGet("/_diag/user/{id:guid}", async (Guid id, IQueryBus q, CancellationToken ct) =>
            {
                var ok = await q.Send(new ContentService.Application.Users.Queries.UserExistsQuery(id), ct);
                return Results.Ok(new { id, exists = ok });
            });

            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseGlobalExceptionHandling();
            app.MapControllers();
            app.Run();
        }
    }
}