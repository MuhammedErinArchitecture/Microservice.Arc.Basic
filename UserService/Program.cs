using FluentValidation;
using FluentValidation.AspNetCore;
using UserService.Infrastructure;
using BuildingBlocks.Abstractions.CQRS;
using UserService.Application.CQRS;
using UserService.Domain.Services;
using UserService.Application.Users;
using ContentService.Api.Middleware;


namespace UserService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddHealthChecks();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddControllers();
            builder.Services.AddFluentValidationAutoValidation();
            builder.Services.AddValidatorsFromAssemblyContaining<UserService.Api.Validation.UserCreateRequestValidator>();
            builder.Services.AddSingleton<IQueryBus, QueryBus>();
            builder.Services.AddScoped<IUserDomainService, UserDomainService>();

            builder.Services.AddUserServiceInfrastructure(builder.Configuration);

            var app = builder.Build();

            app.MapGet("/healthz", () => Results.Ok(new { status = "ok", service = "user-api" }));

            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseGlobalExceptionHandling();
            app.MapControllers();

            app.Run();
        }
    }
}
