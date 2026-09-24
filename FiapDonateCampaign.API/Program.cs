using FiapDonateCampaign.API.Consumers;
using FiapDonateCampaign.API.Middlewares;
using FiapDonateCampaign.Application.Interface;
using FiapDonateCampaign.Application.Services;
using FiapDonateCampaign.Application.Validators;
using FiapDonateCampaign.Infrastructure;
using FiapDonateCampaign.Infrastructure.Identity;
using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionMiddleware>();

app.UseAuthentication(); // sempre ANTES do UseAuthorization
app.UseAuthorization();

app.MapControllers();

app.Run();