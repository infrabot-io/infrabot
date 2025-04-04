using System.DirectoryServices.AccountManagement;

namespace Infrabot.WebUI.Utils
{
    public class ActiveDirectoryAuthenticator
    {
        private readonly string _adServer;
        private readonly string _serviceUser;
        private readonly string _servicePassword;
        private readonly string _domain;

        public ActiveDirectoryAuthenticator(string adServer, string domain, string serviceUser, string servicePassword)
        {
            _adServer = adServer;
            _domain = domain;
            _serviceUser = serviceUser;
            _servicePassword = servicePassword;
        }

        public bool Authenticate(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return false;

            try
            {
                // Connect to the specified Active Directory server using the service account
                using (var context = new PrincipalContext(
                    ContextType.Domain,
                    _adServer,
                    _domain,
                    _serviceUser,
                    _servicePassword))
                {
                    // Validate the target user's credentials
                    return context.ValidateCredentials(username, password);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Authentication failed: {ex.Message}");
                return false;
            }
        }
    }
}
