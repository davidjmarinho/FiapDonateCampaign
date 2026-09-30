using FiapDonateCampaign.API.Consumers;
using FiapDonateCampaign.API.HealthChecks;
using FiapDonateCampaign.API.Middlewares;
using FiapDonateCampaign.Application.Interface;
using FiapDonateCampaign.Application.Services;
using FiapDonateCampaign.Application.Validators;
using FiapDonateCampaign.Infrastructure;
using FiapDonateCampaign.Infrastructure.Data;
using FiapDonateCampaign.Infrastructure.Identity;
using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Prometheus;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Permite que Docker Secrets montados em /run/secrets sobrescrevam appsettings
// e variáveis de ambiente. Arquivos como Jwt__Key e
// ConnectionStrings__DefaultConnection são mapeados para chaves .NET.
builder.Configuration.AddKeyPerFile("/run/secrets", optional: true);

// --- Camadas ---
builder.Services.AddInfrastructure(builder.Configuration); // banco, Identity, repositórios, TokenService
builder.Services.AddScoped<ICampaignService, CampaignService>();
builder.Services.AddScoped<IDonationService, DonationService>();

// --- Validação automática dos DTOs recebidos ---
builder.Services.AddValidatorsFromAssemblyContaining<CampaignRequestValidator>();

// --- Controllers ---
builder.Services.AddControllers();

// --- Health checks ---
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" })
    .AddCheck<SqlServerHealthCheck>("sqlserver", tags: new[] { "ready" })
    .AddCheck<RabbitMqHealthCheck>("rabbitmq", tags: new[] { "ready" });

// --- JWT Authentication ---
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();

// --- RabbitMQ / MassTransit ---
// Publica DoacaoRecebidaEvent (consumido pelo Worker) e consome
// ValorArrecadadoAtualizadoEvent (publicado pelo Worker após creditar uma doação).
var rabbitHost = builder.Configuration["RabbitMq:Host"] ?? "localhost";
var rabbitVirtualHost = builder.Configuration["RabbitMq:VirtualHost"] ?? "/";
var rabbitUsername = builder.Configuration["RabbitMq:Username"] ?? "guest";
var rabbitPassword = builder.Configuration["RabbitMq:Password"] ?? "guest";

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ValorArrecadadoAtualizadoConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitHost, rabbitVirtualHost, h =>
        {
            h.Username(rabbitUsername);
            h.Password(rabbitPassword);
        });

        cfg.ReceiveEndpoint("valor-arrecadado-atualizado-queue", e =>
        {
            e.ConfigureConsumer<ValorArrecadadoAtualizadoConsumer>(context);
            e.UseMessageRetry(r => r.Immediate(3));
        });
    });
});

// --- Swagger com suporte a Bearer token ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "FiapDonateCampaign", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe apenas o token (sem o prefixo 'Bearer ')."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("StartupMigrations");

    try
    {
        await dbContext.Database.MigrateAsync();
    }
    catch (SqlException ex) when (
        ex.Number == 2714 &&
        (ex.Message.Contains("Doacoes", StringComparison.OrdinalIgnoreCase) ||
         ex.Message.Contains("Campaigns", StringComparison.OrdinalIgnoreCase)))
    {
        logger.LogWarning(ex, "Tabelas já existentes no banco compartilhado. Continuando o startup sem reaplicar migration inicial completa.");
    }

    // Em alguns ambientes integrados, as migrações iniciais podem ficar
    // parcialmente aplicadas no banco compartilhado. Garantimos que a tabela
    // Campaigns exista antes de aceitar tráfego.
    await dbContext.Database.ExecuteSqlRawAsync(@"
IF OBJECT_ID(N'[Campaigns]', N'U') IS NULL
BEGIN
    CREATE TABLE [Campaigns] (
        [Id] uniqueidentifier NOT NULL,
        [Titulo] nvarchar(100) NOT NULL,
        [Descricao] nvarchar(500) NOT NULL,
        [DataInicio] datetime2 NOT NULL,
        [DataFim] datetime2 NOT NULL,
        [MetaFinanceira] decimal(18,2) NOT NULL,
        [ValorArrecadado] decimal(18,2) NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Campaigns] PRIMARY KEY ([Id])
    );
END
");

    await dbContext.Database.ExecuteSqlRawAsync(@"
IF OBJECT_ID(N'[Donation]', N'U') IS NULL
BEGIN
    CREATE TABLE [Donation] (
        [Id] uniqueidentifier NOT NULL,
        [CampanhaId] uniqueidentifier NOT NULL,
        [DoadorId] nvarchar(max) NOT NULL,
        [ValorDoacao] decimal(18,2) NOT NULL,
        [DataDoacao] datetime2 NOT NULL,
        CONSTRAINT [PK_Donation] PRIMARY KEY ([Id])
    );
END
");

    await dbContext.Database.ExecuteSqlRawAsync(@"
IF OBJECT_ID(N'[Campanhas]', N'V') IS NULL
BEGIN
    EXEC('CREATE VIEW [Campanhas] AS
          SELECT [Id], [Status], [ValorArrecadado]
          FROM [Campaigns]');
END
");
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpMetrics();

app.UseAuthentication(); // sempre ANTES do UseAuthorization
app.UseAuthorization();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live"),
    ResponseWriter = WriteHealthResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = WriteHealthResponse
});

app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = WriteHealthResponse
});

app.MapMetrics("/metrics");

app.MapControllers();

app.Run();

static Task WriteHealthResponse(HttpContext context, HealthReport report)
{
    context.Response.ContentType = "application/json";

    var payload = new
    {
        status = report.Status.ToString(),
        totalDuration = report.TotalDuration.TotalMilliseconds,
        entries = report.Entries.ToDictionary(
            entry => entry.Key,
            entry => new
            {
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description,
                duration = entry.Value.Duration.TotalMilliseconds,
                data = entry.Value.Data
            })
    };

    return context.Response.WriteAsync(JsonSerializer.Serialize(payload));
}