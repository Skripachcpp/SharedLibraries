using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ConsoleApp8.Rabbit
{
    public class RabbitQueueReader<TMessage> : IDisposable
        where TMessage : class
    {
        private readonly bool _skipFailedMessages;
        private Func<TMessage, bool> _callbackFailedMessages;
        private Func<TMessage, Task> _messageHandler;

        private readonly IRabbitQueueReaderConnection _connectionSettings;

        public RabbitQueueReader(
            Func<TMessage, Task> messageHandler, string hostName, string queueName = null,
            string queueBind = null, bool durable = false, bool exclusive = false, bool autoDelete = false,
            string userName = null, string password = null, string virtualHost = null, int? port = null,
            bool skipFailedMessages = false, Func<TMessage, bool> callbackFailedMessages = null, bool autoStart = true)
            : this(new RabbitQueueReaderConnection()
            {
                HostName = hostName,
                AutoDelete = autoDelete,
                Durable = durable,
                Exclusive = exclusive,
                Password = password,
                Port = port,
                ExchangeName = queueBind,
                QueueName = queueName,
                UserName = userName,
                VirtualHost = virtualHost
            },
                messageHandler, skipFailedMessages, callbackFailedMessages, autoStart)
        { }

        public RabbitQueueReader(
            IRabbitQueueReaderConnection connectionSettings,

            Func<TMessage, Task> messageHandler,
            bool skipFailedMessages = false,
            Func<TMessage, bool> callbackFailedMessages = null,
            bool autoStart = true)
        {
            _messageHandler = messageHandler ?? throw new ArgumentNullException(nameof(messageHandler));

            if (connectionSettings == null) throw new ArgumentNullException(nameof(connectionSettings));
            if (connectionSettings.HostName == null) throw new ArgumentNullException(nameof(connectionSettings.HostName));
            if (connectionSettings.QueueName == null) throw new ArgumentNullException(nameof(connectionSettings.HostName));
            _connectionSettings = connectionSettings;

            _callbackFailedMessages = callbackFailedMessages;
            _skipFailedMessages = skipFailedMessages;

            if (autoStart) Start();
        }

        #region start/stop

        private IConnection _connection;
        private IModel _channel;
        private EventingBasicConsumer _consumer;

        public void Start()
        {
            //если уже запущен то не запускать повторно
            if (_connection != null) return;

            var connection = _connectionSettings;

            if (connection == null) throw new ArgumentNullException(nameof(connection));

            if (!string.IsNullOrWhiteSpace(connection.HostName))
            {
                var factory = new ConnectionFactory { HostName = connection.HostName };
                if (connection.UserName != null) factory.UserName = connection.UserName;
                if (connection.Password != null) factory.Password = connection.Password;
                if (connection.VirtualHost != null) factory.VirtualHost = connection.VirtualHost;
                if (connection.Port != null) factory.Port = connection.Port.Value;

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                var queue = _channel.QueueDeclare(queue: connection.QueueName, durable: connection.Durable, exclusive: connection.Exclusive, autoDelete: connection.AutoDelete, arguments: null);
                if (!string.IsNullOrWhiteSpace(connection.ExchangeName)) _channel.QueueBind(queue, connection.ExchangeName, "");

                _consumer = new EventingBasicConsumer(_channel);

                _consumer.Received += OnCameMessage;
                _channel.BasicConsume(queue: connection.QueueName, autoAck: false, consumer: _consumer);
            }
        }

        public void Stop()
        {
            if (_consumer != null)
            {
                _consumer.Received -= OnCameMessage;
                _consumer = null;
            }

            _channel?.Dispose();
            _channel = null;

            _connection?.Dispose();
            _connection = null;
        }

        #endregion start/stop

        protected virtual TMessage ReadMessage(BasicDeliverEventArgs e)
            => JsonConvert.DeserializeObject<TMessage>(Encoding.UTF8.GetString(e.Body));

        protected virtual void OnCameMessage(object sender, BasicDeliverEventArgs e)
        {
            var message = ReadMessage(e);

            try
            {
                _messageHandler?.Invoke(message)?.GetAwaiter().GetResult();
                _channel.BasicAck(e.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                var processed = _callbackFailedMessages?.Invoke(message) ?? false;
                if (!processed)
                {
                    if (_skipFailedMessages) _channel.BasicAck(e.DeliveryTag, false);
                    else throw;
                }
            }
        }

        private bool _dispose = false;
        public void Dispose()
        {
            if (_dispose) return;

            Stop();

            _messageHandler = null;
            _callbackFailedMessages = null;

            _dispose = true;
        }
    }

    public interface IRabbitQueueReaderConnection : IRabbitAbstractConnection
    {
        string QueueName { get; }
        string ExchangeName { get; }
        bool Durable { get; }
        bool Exclusive { get; }
        bool AutoDelete { get; }
        string VirtualHost { get; }
    }

    public class RabbitQueueReaderConnection : RabbitAbstractConnection, IRabbitQueueReaderConnection
    {
        /// <summary>
        /// Наименование очереди
        /// </summary>
        public string QueueName { get; set; } = null;

        /// <summary>
        /// Binding to exchange
        /// </summary>
        public string ExchangeName { get; set; } = null;

        /// <summary>
        /// Хранить сообщения
        /// </summary>
        public bool Durable { get; set; } = false;

        /// <summary>
        /// Только 1 подключение для очереди
        /// </summary>
        public bool Exclusive { get; set; } = false;

        /// <summary>
        /// Автоматически удалять сообщение из очереди после получения
        /// </summary>
        public bool AutoDelete { get; set; } = false;

        /// <summary>
        /// https://www.rabbitmq.com/vhosts.html
        /// </summary>
        public string VirtualHost { get; set; } = null;
    }
}
