using FiapDonateCampaign.Domain.Enums;
using FiapDonateCampaign.Domain.Exceptions;

namespace FiapDonateCampaign.Domain.Entities
{
    public class Campaign
    {
        public Guid Id { get; private set; }
        public string Titulo { get; private set; }
        public string Descricao { get; private set; }
        public DateTime DataInicio { get; private set; }
        public DateTime DataFim { get; private set; }
        public decimal MetaFinanceira { get; private set; }
        public StatusCampaign Status { get; private set; }

        protected Campaign() { } //EF Core requires a parameterless 

        public Campaign(string titulo, string descricao, DateTime dataInicio, DateTime dataFim, decimal metaFinanceira)
        {
            ValidarRegrasDeCriacao(dataFim, metaFinanceira);

            Id = Guid.NewGuid();
            Titulo = titulo;
            Descricao = descricao;
            DataInicio = dataInicio;
            DataFim = dataFim;
            MetaFinanceira = metaFinanceira;
            Status = StatusCampaign.Ativa;

        }

        private static void ValidarRegrasDeCriacao(DateTime dataFim, decimal metaFinanceira)
        {
            if (dataFim.Date < DateTime.UtcNow.Date)
                throw new DomainException("A data de término não pode estar no passado.");

            if (metaFinanceira <= 0)
                throw new DomainException("A meta financeira deve ser maior que zero.");
        }

        public void Atualizar(string titulo, string descricao, DateTime dataInicio, DateTime dataFim, decimal metaFinanceira, StatusCampaign status)
        {
            ValidarRegrasDeCriacao(dataFim, metaFinanceira);

            Titulo = titulo;
            Descricao = descricao;
            DataInicio = dataInicio;
            DataFim = dataFim;
            MetaFinanceira = metaFinanceira;
            Status = status;
        }
    }
}
