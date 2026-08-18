namespace FiapDonateCampaign.Application.Interface
{
    public interface IEventPublisher
    {
        Task PublishAsync<TEvent>(TEvent @event, string routingKey) where TEvent : class;
    }
}
