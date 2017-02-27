using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using Windows.Security.Credentials;

namespace Discord_Mobile.Services
{
    class LoginService
    {
        public static DiscordSocketClient client; //= new DiscordSocketClient();

        public static PasswordCredential CheckCredit()
        {
            //Set the creditential equal to nothing.
            PasswordCredential loginCredential = null;
            //Make a new reference to the class PassWordVault and name it vault.
            PasswordVault vault = new PasswordVault();

            try
            {
                //Try to find all creditentials save by their resource name and save it into a List.
                var credentialList = vault.FindAllByResource("LoginToken");
                //If there is an element in that list then take the first element.
                if (credentialList.Count > 0)
                {
                    if (credentialList.Count == 1)
                        loginCredential = credentialList[0];
                    /*else
                    {
                        // When there are multiple usernames,
                        // retrieve the default username. If one doesn't
                        // exist, then display UI to have the user select
                        // a default username.


                        credential = vault.Retrieve("LoginToken", null);
                    }*/
                }
            }
            catch { }

            //Return the credential.
            return loginCredential;

        }

        public async Task MakeConnectionAsync(string userToken)
        {
            client = new DiscordSocketClient();

            // Configure the client to use a Bot token, and use our token
            await client.LoginAsync(TokenType.User, userToken);
            // Connect the client to Discord's gateway
            await client.StartAsync();

            await Task.Delay(2000);
        }

        public static void SaveUser(string userToken)
        {
            PasswordVault vault = new PasswordVault();
            vault.Add(new PasswordCredential("LoginToken", "user", userToken));
        }

        public static void DeleteUser()
        {
            var vault = new PasswordVault();
            PasswordCredential loginCredential = CheckCredit();

            if (loginCredential != null)
                vault.Remove(loginCredential);
        }
    }
}
