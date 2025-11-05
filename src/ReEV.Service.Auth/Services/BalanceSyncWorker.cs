using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ReEV.Common.Contracts.Users;
using ReEV.Service.Auth.Models;
using ReEV.Service.Auth.Repositories;
using System.Text;
using System.Text.Json;

namespace ReEV.Service.Auth.Services
{
    public class BalanceSyncWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ConnectionFactory _connectionFactory;
        private readonly ILogger<BalanceSyncWorker> _logger;

        public BalanceSyncWorker(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<BalanceSyncWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
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
            const string queue = "auth-balance-sync";

            await channel.ExchangeDeclareAsync(exchange, ExchangeType.Topic, durable: true);
            await channel.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false);
            await channel.QueueBindAsync(queue, exchange, "user.balance.updated.v1");

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (_, ea) =>
            {
                var bodyBytes = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(bodyBytes);

                var evt = JsonSerializer.Deserialize<UserBalanceUpdatedV1>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })!;

                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var user = await db.Users.FindAsync(evt.UserId);
                    if (user == null)
                    {
                        _logger.LogWarning("User not found for balance sync. UserId: {UserId}", evt.UserId);
                        await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                        return;
                    }

                    // Update Balance và LockedBalance từ MarketplaceService
                    user.Balance = evt.Balance;
                    user.LockedBalance = evt.LockedBalance;

                    await db.SaveChangesAsync();
                    await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);

                    _logger.LogInformation(
                        "Balance synced successfully. UserId: {UserId}, Balance: {Balance}, LockedBalance: {LockedBalance}",
                        evt.UserId, evt.Balance, evt.LockedBalance);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to sync balance. UserId: {UserId}", evt.UserId);
                    // requeue: true nếu muốn xử lý lại; false nếu muốn gửi DLQ
                    await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            await channel.BasicConsumeAsync(queue, autoAck: false, consumer);
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}

