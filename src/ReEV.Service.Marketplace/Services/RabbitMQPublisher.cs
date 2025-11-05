using RabbitMQ.Client;
using System.Text.Json;
using System.Text;
using ReEV.Common.Contracts.Listings;
using ReEV.Common.Contracts.Users;

namespace ReEV.Service.Marketplace.Services
{
    public class RabbitMQPublisher
    {
        private readonly ConnectionFactory _factory;
        private readonly IConnection _connection;
        private readonly IChannel _channel;

        public RabbitMQPublisher(IConfiguration configuration)
        {
            _factory = new ConnectionFactory
            {
                HostName = configuration["RabbitMQ:HostName"] ?? "rabbitmq",
                UserName = configuration["RabbitMQ:UserName"] ?? "guest",
                Password = configuration["RabbitMQ:Password"] ?? "guest",
            };

            _connection = _factory.CreateConnectionAsync().GetAwaiter().GetResult();
            _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
        }

        public async Task PublishListingCreatedAsync(ListingCreatedV1 evt)
        {
            const string exchange = "listing.events";
            await _channel.ExchangeDeclareAsync(exchange, ExchangeType.Topic, durable: true);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(evt));

            var props = new BasicProperties
            {
                DeliveryMode = DeliveryModes.Persistent
            };

            await _channel.BasicPublishAsync(
                exchange: exchange,
                routingKey: "listing.created.v1",
                mandatory: false,
                basicProperties: props,
                body: body
            );
        }

        public async Task PublishListingUpdatedAsync(ListingUpdatedV1 evt)
        {
            const string exchange = "listing.events";
            await _channel.ExchangeDeclareAsync(exchange, ExchangeType.Topic, durable: true);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(evt));

            var props = new BasicProperties
            {
                DeliveryMode = DeliveryModes.Persistent
            };

            await _channel.BasicPublishAsync(
                exchange: exchange,
                routingKey: "listing.updated.v1",
                mandatory: false,
                basicProperties: props,
                body: body
            );
        }

        public async Task PublishUserBalanceUpdatedAsync(UserBalanceUpdatedV1 evt)
        {
            const string exchange = "user.events";
            await _channel.ExchangeDeclareAsync(exchange, ExchangeType.Topic, durable: true);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(evt));

            var props = new BasicProperties
            {
                DeliveryMode = DeliveryModes.Persistent
            };

            await _channel.BasicPublishAsync(
                exchange: exchange,
                routingKey: "user.balance.updated.v1",
                mandatory: false,
                basicProperties: props,
                body: body
            );
        }

        public async ValueTask DisposeAsync()
        {
            await _channel.CloseAsync();
            await _connection.CloseAsync();
        }
    }
}

