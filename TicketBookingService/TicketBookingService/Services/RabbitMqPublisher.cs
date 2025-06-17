using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using TicketBookingService.Abstract;

namespace TicketBookingService.Services
{
    public class RabbitMqPublisher :IRabbitMqPublisher
    {
        private readonly IConfiguration _config;

        public RabbitMqPublisher(IConfiguration config)
        {
            _config = config;
        }

        public async void Publish<T>(T message, string queueName)
        {
            try
            {
                //latest code with version 7.1.2 but getteng exeption in it
                //var factory = new ConnectionFactory { HostName = _config["RabbitMq:Connection"] };
                //using var connection = await factory.CreateConnectionAsync();
                //using var channel = await connection.CreateChannelAsync();

                //await channel.QueueDeclareAsync(queue: queueName, durable: false, exclusive: false, autoDelete: false,
                //    arguments: null);


                //var json = JsonSerializer.Serialize(message);
                //var body = Encoding.UTF8.GetBytes(json);

                //await channel.BasicPublishAsync(exchange: string.Empty, routingKey: queueName, body: body);
                //Console.WriteLine($" [x] Sent {message}");

                var factory = new ConnectionFactory
                {
                    Uri = new Uri(_config["RabbitMq:Connection"]),
                    DispatchConsumersAsync = true
                };

                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();

                // Declare the queue (creates only if not exists)
                channel.QueueDeclare(queue: queueName,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var json = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(json);

                channel.BasicPublish(exchange: "",
                                     routingKey: queueName,
                                     basicProperties: null,
                                     body: body);
            }
            catch (Exception ex) {

                Console.WriteLine(ex.Message);
            }          
        }
    }
}
