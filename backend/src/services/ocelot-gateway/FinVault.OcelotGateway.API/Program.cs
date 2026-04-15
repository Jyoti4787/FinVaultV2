using FinVault.OcelotGateway.API.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using MMLib.SwaggerForOcelot.DependencyInjection;
using System.Text;
using System.Text.Json.Nodes;
// FILE : Program.cs (Ocelot Gateway)
// LAYER: API Gateway
// PATH : ocelot-gateway/.../Program.cs
//
// WHAT IS THIS?
// The "GATEKEEPER" of the whole app.
// Instead of you calling 5 different ports, you just call PORT 5000.
// Ocelot then "Redirects" your call to the right service!
//
// Also validates the JWT token here so broken tokens never reach the microservices.



var builder = WebApplication.CreateBuilder(args);

// 1. ADD OCELOT CONFIG FILE
// NOTE: Do NOT use AddOcelotWithSwaggerSupport(Folder=...) — it scans for
// split ocelot.{key}.json files, finds none, and silently clears all Routes!
// Instead: load ocelot.json directly, and use AddSwaggerForOcelot for the UI.
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// 2. JWT AUTHENTICATION
// The Gateway must also know the Secret to validate incoming tokens
var jwtSecret = builder.Configuration["Jwt:Secret"]!;
var audiences = builder.Configuration.GetSection("Jwt:Audiences").Get<string[]>() ?? new[] { "finvault-api" };
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer   = builder.Configuration["Jwt:Issuer"],
            ValidAudiences = audiences,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerForOcelot(builder.Configuration);

// 3. REGISTER OCELOT
builder.Services.AddOcelot();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.SetIsOriginAllowed(origin => true)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

// 4. HEALTH CHECK
builder.Services.AddHealthChecks();

var app = builder.Build();

// 5. WELCOME PAGE
app.MapGet("/welcome", () => Results.Content("<h1>🚀 FinVault API Gateway is RUNNING!</h1>", "text/html"));

app.UseMiddleware<ExceptionMiddleware>();
app.UseCors("AllowFrontend");

// 6. SWAGGER — must come BEFORE Ocelot so static files aren't intercepted
app.UseSwaggerForOcelotUI(options =>
{
    options.PathToSwaggerGenerator = "/swagger/docs";
}).UseSwagger();

// 7. USE AUTHENTICATION
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

// 8. USE OCELOT MIDDLEWARE — must be LAST
await app.UseOcelot();

app.Run();
