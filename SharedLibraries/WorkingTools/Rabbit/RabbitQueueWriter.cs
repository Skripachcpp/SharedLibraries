using System;
using RabbitMQ.Client;

namespace ConsoleApp8.Rabbit
{
    public class RabbitQueueWriter<TMessage> : RabbitAbstractWriter<TMessage>, IDisposable
        where TMessage : class
    {
        private readonly IRabbitQueueWriterConnection _connectionSettings;

        public RabbitQueueWriter(IRabbitQueueWriterConnection connectionSettings) : base(connectionSettings) 
            => _connectionSettings = connectionSettings;

        protected override void DeclareTarget(IModel channel) 
            => channel.QueueDeclare(queue: _connectionSettings.QueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

        protected override void PushMessage(IModel channel, byte[] body) 
            => channel?.BasicPublish(exchange: string.Empty, routingKey: _connectionSettings.QueueName, basicProperties: null, body: body);
    }

    public interface IRabbitQueueWriterConnection : IRabbitAbstractConnection
    {
        string QueueName { get; }
    }

    public class RabbitQueueWriterConnection : RabbitAbstractConnection, IRabbitQueueWriterConnection
    {
        /// <summary>
        /// Наименование очереди
        /// </summary>
        public string QueueName { get; set; } = null;
    }
}