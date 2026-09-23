using FiapDonateCampaign.Application.Interface;
using FiapDonateCampaign.Domain.Interfaces;
using FiapDonateCampaign.Infrastructure.Auth;
using FiapDonateCampaign.Infrastructure.Data;
using FiapDonateCampaign.Infrastructure.Identity;
using FiapDonateCampaign.Infrastructure.Messaging;
using FiapDonateCampaign.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FiapDonateCampaign.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // DbContext das entidades da própria API — Campaign é dona das migrations deste.
        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

        // DbContext só para leitura do Identity — aponta pro mesmo banco físico,
        // mas a Campaign nunca gera migration a partir dele.
        services.AddDbContext<IdentityStoreDbContext>(options => options.UseSqlServer(connectionString));

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireNonAlphanumeric = false;
        })
            .AddEntityFrameworkStores<IdentityStoreDbContext>() // repare: aponta pro IdentityStoreDbContext, não pro AppDbContext
            .AddDefaultTokenProviders();

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ICampaignRepository, CampaignRepository>();
        services.AddScoped<IDonationRepository, DonationRepository>();
        services.AddScoped<IEventPublisher, LogEventPublisher>();

        return services;
    }
}