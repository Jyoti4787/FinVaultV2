using FinVault.IdentityService.API.Middleware;
using FinVault.IdentityService.Domain.Interfaces;
using FinVault.IdentityService.Infrastructure;
using FinVault.IdentityService.Infrastructure.Messaging.Publishers;
using FinVault.IdentityService.Infrastructure.Persistence;
using FinVault.IdentityService.Infrastructure.Persistence.Repositories;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Threading.RateLimiting;
using Serilog;
using FinVault.Shared.Security;

var builder = WebApplication.CreateBuilder(args);

// Load Secrets from KeyVault (Mocked for Dev)
builder.Configuration.AddMockKeyVault();

// ── 0. CONFIGURE SERILOG ─────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/identity-service-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Starting FinVault Identity Service...");

    //  1. DATABASE 
    builder.Services.AddDbContext<IdentityDbContext>(options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("IdentityDb")));

    //  2. REPOSITORIES 
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
    builder.Services.AddScoped<IOTPCodeRepository, OTPCodeRepository>();
    builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
    builder.Services.AddScoped<IPendingRegistrationRepository, PendingRegistrationRepository>();
    // Profile picture stored in SQL Server — no MongoDB needed
    builder.Services.AddScoped<IProfilePictureRepository, ProfilePictureRepository>();

    //  3. MEDIATR 
    // Scan Application assembly for IRequestHandlers (commands/queries)
    // Scan Infrastructure assembly for INotificationHandlers (domain event publishers)
    // Use a HashSet to prevent duplicate registrations — Infrastructure references Application,
    // so without deduplication both assemblies would register Application handlers twice,
    // causing each command handler to execute twice (e.g. double OTP emails).
    builder.Services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssemblies(
            typeof(FinVault.IdentityService.Application.Commands.RegisterUser.RegisterUserCommand).Assembly,
            typeof(UserRegisteredPublisher).Assembly
        );
    });

    //  4. MASSTRANSIT + RABBITMQ 
    builder.Services.AddMassTransit(x =>
    {
        x.UsingRabbitMq((ctx, cfg) =>
        {
            cfg.Host(builder.Configuration["RabbitMQ:Host"], h =>
            {
                h.Username(builder.Configuration["RabbitMQ:Username"]!);
                h.Password(builder.Configuration["RabbitMQ:Password"]!);
            });
        });
    });

    //  5. JWT AUTHENTICATION 
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

    //  5b. RATE LIMITING — bot/brute-force protection 
    // Login: max 10 requests per 60s per IP
    // Register: max 5 requests per 60s per IP
    builder.Services.AddRateLimiter(options =>
    {
        options.AddFixedWindowLimiter("login", o =>
        {
            o.Window            = TimeSpan.FromSeconds(int.Parse(builder.Configuration["RateLimiting:LoginWindowSeconds"] ?? "60"));
            o.PermitLimit       = int.Parse(builder.Configuration["RateLimiting:LoginMaxRequests"] ?? "10");
            o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            o.QueueLimit        = 0;
        });
        options.AddFixedWindowLimiter("register", o =>
        {
            o.Window            = TimeSpan.FromSeconds(int.Parse(builder.Configuration["RateLimiting:RegisterWindowSeconds"] ?? "60"));
            o.PermitLimit       = int.Parse(builder.Configuration["RateLimiting:RegisterMaxRequests"] ?? "5");
            o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            o.QueueLimit        = 0;
        });
        options.AddFixedWindowLimiter("otp", o =>
        {
            o.Window            = TimeSpan.FromSeconds(60);
            o.PermitLimit       = 5;
            o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            o.QueueLimit        = 0;
        });
        options.RejectionStatusCode = 429;
    });

    //  6. CONTROLLERS + SWAGGER 
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "FinVault — Identity Service", Version = "v1" });
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below. Example: 'Bearer 12345abcdef'",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT"
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            { 
                new OpenApiSecurityScheme 
                { 
                    Reference = new OpenApiReference 
                    { 
                        Type = ReferenceType.SecurityScheme, 
                        Id = "Bearer" 
                    } 
                }, 
                Array.Empty<string>() 
            }
        });
    });

    //  7. HEALTH CHECK 
    builder.Services.AddHealthChecks().AddDbContextCheck<IdentityDbContext>(name: "Identity Database");

    var app = builder.Build();

    // 8. AUTO CREATE TABLES ON STARTUP 
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        db.Database.Migrate();
    }

    //  9. MIDDLEWARE PIPELINE
    app.UseMiddleware<ExceptionMiddleware>();
    app.UseRateLimiter();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FinVault Identity Service v1");
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
    Log.Fatal(ex, "Identity Service terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}