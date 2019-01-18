using System;
using RabbitMQ.Client;

namespace ConsoleApp8.Rabbit
{
    public class RabbitExchangeWriter<TMessage> : RabbitAbstractWriter<TMessage>, IDisposable
        where TMessage : class
    {
        private readonly IRabbitExchangeWriterConnection _connectionSettings;

        public RabbitExchangeWriter(IRabbitExchangeWriterConnection connectionSettings) : base(connectionSettings)
        {
            if (connectionSettings?.ExchangeName == null) throw new ArgumentNullException(nameof(connectionSettings.ExchangeName));
            _connectionSettings = connectionSettings;
        }

        protected override void DeclareTarget(IModel channel) 
            => channel.ExchangeDeclare(exchange: _connectionSettings.ExchangeName, type: "fanout");

        protected override void PushMessage(IModel channel, byte[] body) 
            => channel?.BasicPublish(exchange: _connectionSettings.ExchangeName, routingKey: String.Empty, basicProperties: null, body: body);
    }

    public interface IRabbitExchangeWriterConnection : IRabbitAbstractConnection
    {
        string ExchangeName { get; }
    }

    public class RabbitExchangeWriterConnection : RabbitAbstractConnection, IRabbitExchangeWriterConnection
    {
        /// <summary>
        /// Наименование exchange
        /// </summary>
        public string ExchangeName { get; set; } = null;
    }
}