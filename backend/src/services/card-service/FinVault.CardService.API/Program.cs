using FinVault.CardService.API.Middleware;
using FinVault.CardService.Application.Behaviors;
using FinVault.CardService.Domain.Interfaces;
using FinVault.CardService.Infrastructure.HostedServices;
using FinVault.CardService.Infrastructure.Messaging.Consumers;
using FinVault.CardService.Infrastructure.Messaging.Publishers;
using FinVault.CardService.Infrastructure.Persistence;
using FinVault.CardService.Infrastructure.Persistence.Repositories;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ── 0. CONFIGURE SERILOG ─────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/card-service-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Starting FinVault Card Service...");

    builder.Services.AddDbContext<CardDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("CardDb")));

    builder.Services.AddScoped<ICreditCardRepository, CreditCardRepository>();
    builder.Services.AddScoped<ICardIssuerRepository, CardIssuerRepository>();

    builder.Services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssembly(typeof(FinVault.CardService.Application.Commands.AddCard.AddCardCommand).Assembly);
        cfg.RegisterServicesFromAssembly(typeof(CardAddedPublisher).Assembly);
        cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    });

    builder.Services.AddMassTransit(x =>
    {
        x.AddConsumer<PaymentCompletedConsumer>();
        x.UsingRabbitMq((ctx, cfg) =>
        {
            cfg.Host(builder.Configuration["RabbitMQ:Host"], h =>
            {
                h.Username(builder.Configuration["RabbitMQ:Username"]!);
                h.Password(builder.Configuration["RabbitMQ:Password"]!);
            });
            cfg.ReceiveEndpoint("card-service.PaymentCompleted", e => e.ConfigureConsumer<PaymentCompletedConsumer>(ctx));
        });
    });

    var jwtSecret = builder.Configuration["Jwt:Secret"]!;
    var audiences = builder.Configuration.GetSection("Jwt:Audiences").Get<string[]>() ?? new[] { "finvault-api" };
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer           = true,
                ValidateAudience         = true,
                ValidateLifetime         = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer   = builder.Configuration["Jwt:Issuer"],
                ValidAudiences = audiences,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
            };
        });

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "FinVault — Card Service", Version = "v1" });
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization", Type = SecuritySchemeType.ApiKey, Scheme = "Bearer", BearerFormat = "JWT", In = ParameterLocation.Header
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() }
        });
    });

    builder.Services.AddHealthChecks().AddDbContextCheck<CardDbContext>(name: "Card Database");
    builder.Services.AddHostedService<CardExpiryCheckHostedService>();

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<CardDbContext>();
        db.Database.Migrate();
    }

    app.UseMiddleware<ExceptionMiddleware>();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FinVault Card Service v1");
        c.RoutePrefix = string.Empty;
    });

    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHealthChecks("/health");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Card Service terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
