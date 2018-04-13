using WorkingTools.Factories.WebRequestFactoryPart;

namespace WorkingTools.Factories.FtpWebRequestFacrotyPart
{
    public interface IServerConnection
    {
        string Server { get; }
        LoginPass User { get; }
    }

    public class ServerConnection : IServerConnection
    {
        public ServerConnection()
            : this(null, null)
        { }

        public ServerConnection(string server, LoginPass user)
        {
            Server = server;
            User = user;
        }

        public string Server { get; set; }
        public LoginPass User { get; set; }
    }
}