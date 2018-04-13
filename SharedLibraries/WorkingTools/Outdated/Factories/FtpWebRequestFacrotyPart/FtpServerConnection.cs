namespace WorkingTools.Factories.FtpWebRequestFacrotyPart
{
    public interface IFtpServerConnection : IServerConnection
    {
        bool UsePassive { get; set; }
    }

    public class FtpServerConnection : ServerConnection, IFtpServerConnection
    {
        public bool UsePassive { get; set; }
    }
}