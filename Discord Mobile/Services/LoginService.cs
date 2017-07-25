using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using Windows.Security.Credentials;

namespace Discord_Mobile.Services
{
    class LoginService
    {
        public static DiscordSocketClient client;

        public static PasswordCredential GetCredit()
        {
            //Set the creditential equal to nothing.
            PasswordCredential loginCredential = null;
            //Make a new reference to the class PassWordVault and name it vault.
            PasswordVault vault = new PasswordVault();

            try
            {
                loginCredential = vault.Retrieve("LoginToken", "user");
            }
            catch
            {
                loginCredential = null;
            }
            //Return the credential.
            return loginCredential;

        }

        public async Task MakeConnectionAsync(string userToken)
        {
            DiscordSocketConfig Config = new DiscordSocketConfig { MessageCacheSize = 60 };
            client = new DiscordSocketClient(Config);

            // Configure the client to use a Bot token, and use our token
            await client.LoginAsync(TokenType.User, userToken);
            // Connect the client to Discord's gateway
            await client.StartAsync();
        }

        public static void SaveUser(string userToken)
        {
            PasswordVault vault = new PasswordVault();
            vault.Add(new PasswordCredential("LoginToken", "user", userToken));
        }

        public static void DeleteUser()
        {
            var vault = new PasswordVault();
            PasswordCredential loginCredential = GetCredit();

            if (loginCredential != null)
                vault.Remove(loginCredential);
        }
    }
}
