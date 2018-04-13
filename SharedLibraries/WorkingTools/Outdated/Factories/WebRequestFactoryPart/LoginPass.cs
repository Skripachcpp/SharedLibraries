using System.Net;

namespace WorkingTools.Factories.WebRequestFactoryPart
{
    public interface ILoginPass
    {
        string Login { get; }
        string Password { get; }
    }

    public static class LoginPassExtension
    {
        public static ICredentials ToCredentials(this ILoginPass loginPass)
        {
            if (loginPass == null) return CredentialCache.DefaultCredentials;
            else return new NetworkCredential(loginPass.Login, loginPass.Password);
        }
    }

    public class LoginPass : ILoginPass
    {
        public LoginPass()
        { }

        public LoginPass(string login, string password)
        {
            Login = login;
            Password = password;
        }

        public string Login { get; set; }
        public string Password { get; set; }
    }
}