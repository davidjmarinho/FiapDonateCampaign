using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FiapDonateCampaign.Domain.Interfaces;
using FiapDonateCampaign.Infrastructure.Auth;
using FiapDonateCampaign.Infrastructure.Data;
using FiapDonateCampaign.Infrastructure.Identity;
using FiapDonateCampaign.Infrastructure.Repositories;

namespace FiapDonateCampaign.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireNonAlphanumeric = false; // simplifica pra desenvolvimento/testes
        })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ICampaignRepository, CampaignRepository>();

        return services;
    }
}