using FinVault.PaymentService.API.Middleware;
using FinVault.PaymentService.Domain.Interfaces;
using FinVault.PaymentService.Infrastructure.Messaging.Consumers;
using FinVault.PaymentService.Infrastructure.Messaging.Sagas;
using FinVault.PaymentService.Infrastructure.Persistence;
using FinVault.PaymentService.Infrastructure.Persistence.Repositories;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
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
    .WriteTo.File("logs/payment-service-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Starting FinVault Payment Service...");

    // 1. DATABASE
    builder.Services.AddDbContext<PaymentDbContext>(options =>
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("PaymentDb"));
        options.ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    });

    // 2. REPOSITORIES
    builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

    // 2b. HTTP CLIENT — for OTP verification calls to identity-service
    builder.Services.AddHttpClient("IdentityService");
    builder.Services.AddScoped<FinVault.PaymentService.Domain.Interfaces.IPaymentOtpVerifier,
                               FinVault.PaymentService.Infrastructure.OtpVerification.IdentityOtpVerifier>();

    // 3. MEDIATR
    builder.Services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssembly(typeof(FinVault.PaymentService.Application.Commands.ProcessPayment.ProcessPaymentCommand).Assembly);
    });

    // 4. MASSTRANSIT + RABBITMQ + DLQ + SAGA
    builder.Services.AddMassTransit(x =>
    {
        x.AddConsumer<PaymentRequestedConsumer>();
        x.AddConsumer<PaymentRollbackConsumer>();

        // ── SAGA REGISTRATION ─────────────────────────────────────────
        x.AddSagaStateMachine<PaymentStateMachine, PaymentState>()
            .EntityFrameworkRepository(r =>
            {
                r.ExistingDbContext<PaymentDbContext>();
                r.UseSqlServer();
            });

        x.UsingRabbitMq((ctx, cfg) =>
        {
            cfg.Host(builder.Configuration["RabbitMQ:Host"], h =>
            {
                h.Username(builder.Configuration["RabbitMQ:Username"]!);
                h.Password(builder.Configuration["RabbitMQ:Password"]!);
            });

            // ── DLQ & RETRY POLICY ────────────────────────────────────
            cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));

            cfg.ReceiveEndpoint("payment-service.PaymentRequested", e => e.ConfigureConsumer<PaymentRequestedConsumer>(ctx));
            cfg.ReceiveEndpoint("payment-service.Rollback", e => e.ConfigureConsumer<PaymentRollbackConsumer>(ctx));

            // ── SAGA ENDPOINT ─────────────────────────────────────────
            cfg.ReceiveEndpoint("payment-service.saga", e => e.ConfigureSaga<PaymentState>(ctx));
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
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "FinVault — Payment Service", Version = "v1" });
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { Name = "Authorization", Type = SecuritySchemeType.ApiKey, Scheme = "Bearer", BearerFormat = "JWT", In = ParameterLocation.Header });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement { { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() } });
    });

    builder.Services.AddHealthChecks().AddDbContextCheck<PaymentDbContext>(name: "Payment Database");

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
        db.Database.Migrate();
    }

    app.UseMiddleware<ExceptionMiddleware>();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FinVault Payment Service v1");
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
    Log.Fatal(ex, "Payment Service terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
