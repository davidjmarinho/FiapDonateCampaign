using Microsoft.AspNetCore.Identity;

namespace FiapDonateCampaign.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string Nome { get; set; } = string.Empty;
    }
}
