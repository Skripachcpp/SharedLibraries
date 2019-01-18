namespace ConsoleApp8.Rabbit
{
    public interface IRabbitAbstractConnection
    {
        string UserName { get; }
        string Password { get; }
        string HostName { get; }
        int? Port { get; }
    }

    public abstract class RabbitAbstractConnection
    {
        /// <summary>
        /// Имя пользователя
        /// </summary>
        public string UserName { get; set; } = null;

        /// <summary>
        /// Пароль
        /// </summary>
        public string Password { get; set; } = null;

        /// <summary>
        /// Сервер
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        /// Порт (если null то порт по умолчанию)
        /// </summary>
        public int? Port { get; set; } = null;
    }
}