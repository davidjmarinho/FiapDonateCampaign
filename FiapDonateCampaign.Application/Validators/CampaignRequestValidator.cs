using FiapDonateCampaign.Application.DTOs;
using FluentValidation;

namespace FiapDonateCampaign.Application.Validators
{
    public class CampaignRequestValidator : AbstractValidator<CampaignRequestDto>
    {
        public CampaignRequestValidator() 
        {
            RuleFor(x => x.Titulo).NotEmpty().MaximumLength(150);
            RuleFor(x => x.Descricao).NotEmpty();
            RuleFor(x => x.DataFim).GreaterThan(x => x.DataInicio)
                .WithMessage("A data de término deve ser posterior à data de início.");
            RuleFor(x => x.MetaFinanceira).GreaterThan(0);
        }
    }
}
