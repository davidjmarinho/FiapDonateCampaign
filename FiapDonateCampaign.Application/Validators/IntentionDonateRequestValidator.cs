using FiapDonateCampaign.Application.DTOs;
using FluentValidation;

namespace FiapDonateCampaign.Application.Validators
{
    public class IntentionDonateRequestValidator : AbstractValidator<IntentionDonateRequestDto>
    {
        public IntentionDonateRequestValidator()
        {
            RuleFor(x => x.IdCampanha).NotEmpty();
            RuleFor(x => x.ValorDoacao).GreaterThan(0);
        }
    }
}
