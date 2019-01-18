using System;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace ConsoleApp8.Rabbit
{
    public abstract class RabbitAbstractWriter<TMessage>
    {
        private readonly IRabbitAbstractConnection _connectionSettings;

        private IConnection _connection;
        private IModel _channel;

        protected RabbitAbstractWriter(IRabbitAbstractConnection connectionSettings)
        {
            if (connectionSettings == null) throw new ArgumentNullException(nameof(connectionSettings));
            if (connectionSettings.HostName == null) throw new ArgumentNullException(nameof(connectionSettings.HostName));

            _connectionSettings = connectionSettings;
        }

        private bool _isInit;
        private void Init()
        {
            if (_isInit) return;

            var factory = new ConnectionFactory() { HostName = _connectionSettings.HostName };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            DeclareTarget(_channel);

            _isInit = true;
        }

        public void Push(TMessage message)
        {
            if (!_isInit) Init();

            var json = JsonConvert.SerializeObject(message, settings: new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }, formatting: Formatting.None);
            var body = Encoding.UTF8.GetBytes(json);

            PushMessage(_channel, body);
        }

        public virtual void Dispose()
        {
            _connection?.Dispose();
            _connection = null;
            _channel?.Dispose();
            _channel = null;
        }


        protected abstract void DeclareTarget(IModel channel);
        protected abstract void PushMessage(IModel channel, byte[] body);
    }
}