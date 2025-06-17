namespace TicketBookingService.Abstract
{
    public interface IRabbitMqPublisher
    {
        void Publish<T>(T message, string queueName);
    }
}
