using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;
using EventosVivos.API.Middleware;
using EventosVivos.Application;
using EventosVivos.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/eventosvivos-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting EventosVivos.API");

    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddApplication();

    JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

    var jwtKey = builder.Configuration["Jwt:Key"]
        ?? throw new InvalidOperationException("Jwt:Key not configured.");
    var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "EventosVivos.API";

    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(opt =>
        {
            opt.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer           = true,
                ValidateAudience         = true,
                ValidateLifetime         = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer              = jwtIssuer,
                ValidAudience            = jwtIssuer,
                IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
            };
        });

    builder.Services.AddAuthorization(opt =>
        opt.AddPolicy("AdminOnly", p => p.RequireClaim("rol", "admin")));

    builder.Services.AddCors(opt =>
        opt.AddPolicy("DevCors", p => p
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod()));

    var globalLimit  = builder.Configuration.GetValue("RateLimiting:Global:PermitLimit", 100);
    var globalWindow = builder.Configuration.GetValue("RateLimiting:Global:WindowSeconds", 60);
    var authLimit    = builder.Configuration.GetValue("RateLimiting:Auth:PermitLimit", 5);
    var authWindow   = builder.Configuration.GetValue("RateLimiting:Auth:WindowSeconds", 60);

    builder.Services.AddRateLimiter(opt =>
    {
        opt.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        // Global limit: per client IP (values from RateLimiting:Global).
        opt.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(http =>
        {
            var ip = http.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return RateLimitPartition.GetFixedWindowLimiter(ip, _ =>
                new FixedWindowRateLimiterOptions
                {
                    PermitLimit = globalLimit,
                    Window      = TimeSpan.FromSeconds(globalWindow),
                    QueueLimit  = 0
                });
        });

        // Stricter limit for auth endpoints, per IP (values from RateLimiting:Auth).
        opt.AddPolicy("auth", http =>
        {
            var ip = http.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return RateLimitPartition.GetFixedWindowLimiter($"auth-{ip}", _ =>
                new FixedWindowRateLimiterOptions
                {
                    PermitLimit = authLimit,
                    Window      = TimeSpan.FromSeconds(authWindow),
                    QueueLimit  = 0
                });
        });

        // Same JSON error shape as ExceptionHandlingMiddleware.
        opt.OnRejected = async (ctx, token) =>
        {
            ctx.HttpContext.Response.ContentType = "application/json";

            if (ctx.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                ctx.HttpContext.Response.Headers.RetryAfter =
                    ((int)retryAfter.TotalSeconds).ToString();

            var json = JsonSerializer.Serialize(new
            {
                error   = "RATE_LIMIT_EXCEEDED",
                message = "Too many requests. Please try again later."
            });
            await ctx.HttpContext.Response.WriteAsync(json, token);
        };
    });

    builder.Services.AddControllers()
        .AddJsonOptions(opt =>
            opt.JsonSerializerOptions.Converters.Add(
                new System.Text.Json.Serialization.JsonStringEnumConverter()));
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "EventosVivos API", Version = "v1" });

        var scheme = new OpenApiSecurityScheme
        {
            Name         = "Authorization",
            Type         = SecuritySchemeType.Http,
            Scheme       = "bearer",
            BearerFormat = "JWT",
            In           = ParameterLocation.Header,
            Description  = "Enter the JWT token. Example: Bearer {token}"
        };
        c.AddSecurityDefinition("Bearer", scheme);
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                Array.Empty<string>()
            }
        });
    });

    var app = builder.Build();

    app.UseMiddleware<ExceptionHandlingMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseCors("DevCors");
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
