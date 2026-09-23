using FiapDonateCampaign.Domain.Exceptions;

namespace FiapDonateCampaign.Domain.Entities
{
    public class Donation
    {
        public Guid Id { get; private set; }
        public Guid CampanhaId { get; private set; }
        public string DoadorId { get; private set; }
        public decimal ValorDoacao { get; private set; }
        public DateTime DataDoacao { get; private set; }

        protected Donation() { } // EF Core

        public Donation(Guid campanhaId, string doadorId, decimal valorDoacao)
        {
            if (valorDoacao <= 0)
                throw new DomainException("O valor da doação deve ser maior que zero.");

            Id = Guid.NewGuid();
            CampanhaId = campanhaId;
            DoadorId = doadorId;
            ValorDoacao = valorDoacao;
            DataDoacao = DateTime.UtcNow;
        }
    }
}
