// ==================================================================
// FILE : Program.cs (Notification Service)
// ==================================================================

using FinVault.NotificationService.API.Middleware;
using FinVault.NotificationService.Application.Interfaces;
using FinVault.NotificationService.Domain.Interfaces;
using FinVault.NotificationService.Infrastructure.Email;
using FinVault.NotificationService.Infrastructure.Messaging.Consumers;
using FinVault.NotificationService.Infrastructure.Persistence;
using FinVault.NotificationService.Infrastructure.Persistence.Repositories;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
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
    .WriteTo.File("logs/notification-service-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Starting FinVault Notification Service...");

    // 1. DATABASE
    builder.Services.AddDbContext<NotificationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("NotificationDb")));

    // 2. REPOSITORIES
    builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
    builder.Services.AddScoped<ISupportTicketRepository, SupportTicketRepository>();

    // 2b. EMAIL SENDER
    builder.Services.AddSingleton<IEmailService, GmailEmailService>();
    builder.Services.AddSingleton<IEmailSender>(sp => (GmailEmailService)sp.GetRequiredService<IEmailService>());

    // 3. MEDIATR
    builder.Services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssembly(typeof(FinVault.NotificationService.Application.Commands.SendNotification.SendNotificationCommand).Assembly);
    });

    // 4. MASSTRANSIT + RABBITMQ + DLQ
    builder.Services.AddMassTransit(x =>
    {
        x.AddConsumer<CardAddedConsumer>();
        x.AddConsumer<PaymentCompletedConsumer>();
        x.AddConsumer<StatementGeneratedConsumer>();
        x.AddConsumer<OtpRequestedConsumer>();

        x.UsingRabbitMq((ctx, cfg) =>
        {
            cfg.Host(builder.Configuration["RabbitMQ:Host"], h =>
            {
                h.Username(builder.Configuration["RabbitMQ:Username"]!);
                h.Password(builder.Configuration["RabbitMQ:Password"]!);
            });

            // ── DLQ & RETRY POLICY ────────────────────────────────────
            cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));

            cfg.ReceiveEndpoint("notification-service.CardAdded", e =>
            {
                e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                e.ConfigureConsumer<CardAddedConsumer>(ctx);
            });
            cfg.ReceiveEndpoint("notification-service.PaymentCompleted", e =>
            {
                e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                e.ConfigureConsumer<PaymentCompletedConsumer>(ctx);
            });
            cfg.ReceiveEndpoint("notification-service.StatementGenerated", e =>
            {
                e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                e.ConfigureConsumer<StatementGeneratedConsumer>(ctx);
            });
            cfg.ReceiveEndpoint("notification-service.OtpRequested", e =>
            {
                // OTP emails are critical — retry 5 times with exponential backoff
                // 5s → 10s → 20s → 40s → 80s
                e.UseMessageRetry(r => r.Exponential(5, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(80), TimeSpan.FromSeconds(5)));
                e.ConfigureConsumer<OtpRequestedConsumer>(ctx);
            });
        });
    });

    // 5. AUTH
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
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "FinVault — Notification Service", Version = "v1" });
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { Name = "Authorization", Type = SecuritySchemeType.ApiKey, Scheme = "Bearer", BearerFormat = "JWT", In = ParameterLocation.Header });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement { { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() } });
    });

    builder.Services.AddHealthChecks().AddDbContextCheck<NotificationDbContext>(name: "Notification Database");

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
        db.Database.Migrate();
    }

    app.UseMiddleware<ExceptionMiddleware>();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FinVault Notification Service v1");
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
    Log.Fatal(ex, "Notification Service terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
