using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ReEV.Common.Contracts.Users;
using ReEV.Service.Transaction.Models;
using ReEV.Service.Transaction.Repositories;
using System.Text;
using System.Text.Json;

namespace ReEV.Service.Transaction.Services
{
    public class UserSyncWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ConnectionFactory _connectionFactory;

        public UserSyncWorker(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _connectionFactory = new ConnectionFactory
            {
                HostName = configuration["RabbitMQ:HostName"] ?? "rabbitmq",
                UserName = configuration["RabbitMQ:UserName"] ?? "guest",
                Password = configuration["RabbitMQ:Password"] ?? "guest",
            };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var connection = await _connectionFactory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();

            const string exchange = "user.events";
            const string queue = "transaction-users-sync";

            await channel.ExchangeDeclareAsync(exchange, ExchangeType.Topic, durable: true);
            await channel.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false);
            await channel.QueueBindAsync(queue, exchange, "user.upserted.v1");

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (_, ea) =>
            {
                var bodyBytes = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(bodyBytes);

                var evt = JsonSerializer.Deserialize<UserUpsertedV1>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })!;

                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var user = await db.Users.FindAsync(evt.UserId);
                    if (user is null)
                    {
                        db.Users.Add(new User
                        {
                            Id = evt.UserId,
                            FullName = evt.FullName,
                            PhoneNumber = evt.PhoneNumber,
                            Email = evt.Email,
                        });
                    }
                    else
                    {
                        user.FullName = evt.FullName;
                        user.PhoneNumber = evt.PhoneNumber;
                        user.Email = evt.Email;
                    }

                    await db.SaveChangesAsync();
                    await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                }
                catch
                {
                    // requeue: true nếu muốn xử lý lại; false nếu muốn gửi DLQ
                    await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            await channel.BasicConsumeAsync(queue, autoAck: false, consumer);
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}
