using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using TicketBookingService.Models;
using TicketBookingService.Abstract;

namespace TicketBookingService.Services
{
    public class EmailWorker :BackgroundService 
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _config;

        public EmailWorker(IServiceProvider serviceProvider, IConfiguration config)
        {
            _serviceProvider = serviceProvider;
            _config = config;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory
            {
                Uri = new Uri(_config["RabbitMq:Connection"]),
                DispatchConsumersAsync = true
            };

            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "emailQueue", durable: true, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    var email = JsonSerializer.Deserialize<EmailMessageModel>(json);

                    using var scope = _serviceProvider.CreateScope();
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                    await emailService.SendEmailAsync(email.To, email.Subject, email.Body);
                    Console.WriteLine($"✅ Email sent to: {email.To}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error processing email: {ex.Message}");
                }
            };

            channel.BasicConsume(queue: "emailQueue", autoAck: true, consumer);
            return Task.CompletedTask;
        }
    }
}
