// ATENÇÃO: o namespace abaixo é "FiapDonateWorker.Api.Events", não
// "FiapDonateCampaign.Application.Event". Isso é intencional, não um erro.
//
// O MassTransit roteia mensagens no RabbitMQ pela identidade do tipo CLR
// (namespace + nome) usando a topologia padrão. A API e o Worker são
// repositórios distintos, cada um com seu próprio bus configurado
// independentemente - para que ambos os processos calculem o mesmo nome de
// exchange sem depender de um pacote de contratos compartilhado entre
// repositórios, este tipo precisa ter o namespace e o nome IDÊNTICOS ao tipo
// consumido pelo Worker (FiapDonateWorker.Api.Events.DoacaoRecebidaEvent, em
// FiapDonateWorker/src/FiapDonateWorker.Api/Events/DoacaoRecebidaEvent.cs).
// Qualquer divergência de namespace ou nome faz a mensagem publicada aqui
// nunca chegar à fila do Worker, silenciosamente.
namespace FiapDonateWorker.Api.Events;

public record DoacaoRecebidaEvent(
    Guid DoacaoId,
    Guid IdCampanha,
    decimal ValorDoacao,
    DateTimeOffset DataHoraRecebida);
