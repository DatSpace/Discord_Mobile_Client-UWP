using Discord_Mobile.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Security.Credentials;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Discord_Mobile.ViewModels
{
    class LoginViewModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public async Task UserLogin()
        {
            LoginService loginService = new LoginService();

            PasswordCredential loginCredential = LoginService.CheckCredit();

            //If the login page is not visible
            if (LoginUIVisibility == Visibility.Collapsed)
            {
                //If there is a credential stored
                if (loginCredential != null)
                {
                    //Get the password into the credential object.
                    loginCredential.RetrievePassword();
                    //And try to connect!
                    try
                    {
                        await loginService.MakeConnectionAsync(loginCredential.Password);
                        Frame rootFrame = Window.Current.Content as Frame;
                        rootFrame.Navigate(typeof(ChatPage));
                    }
                    catch
                    {
                        LoginStatusTextBlock = "Can't connect at the moment. Sorry :/";
                    }
                }
                // There is no credential stored in the locker.
                else
                {
                    // Display UI to get user credentials.
                    LoginUIVisibility = Visibility.Visible;
                }
            }
            //That means that the user doesnt have any password saved and the login page is visible
            else
            {
                try
                {
                    LoginButtonIsEnabled = false;
                    await loginService.MakeConnectionAsync(TokenTextBox);
                    if (RememberMeIsChecked)
                        LoginService.SaveUser(TokenTextBox);
                    Frame rootFrame = Window.Current.Content as Frame;
                    rootFrame.Navigate(typeof(ChatPage));
                }
                catch
                {
                    LoginButtonIsEnabled = true;
                    ConnectionErrorTextBlock = "Error! Please re-check the token!";
                }
            }



        }


        //#################< START OF PROPERTIES >######################

        private Visibility loginUIVisibility = Visibility.Collapsed;
        public Visibility connectingVisibility = Visibility.Visible;

        public Visibility LoginUIVisibility
        {
            get
            {
                return loginUIVisibility;
            }

            set
            {
                if (value != loginUIVisibility)
                {
                    loginUIVisibility = value;
                    connectingVisibility = Visibility.Collapsed;
                    NotifyPropertyChanged("LoginUIVisibility");
                    NotifyPropertyChanged("connectingVisibility");
                }
            }
        }

        private bool loginButtonIsEnabled = true;

        public bool LoginButtonIsEnabled
        {
            get
            {
                return loginButtonIsEnabled;
            }

            set
            {
                if (value != loginButtonIsEnabled)
                {
                    loginButtonIsEnabled = value;
                    NotifyPropertyChanged("LoginButtonIsEnabled");
                }
            }
        }

        private string connectionErrorTextBlock = "";

        public string ConnectionErrorTextBlock
        {
            get
            {
                return connectionErrorTextBlock;
            }

            set
            {
                if (value != connectionErrorTextBlock)
                {
                    connectionErrorTextBlock = value;
                    NotifyPropertyChanged("ConnectionErrorTextBlock");
                }
            }
        }

        private string loginStatusTextBlock = "Connecting...";

        public string LoginStatusTextBlock
        {
            get
            {
                return loginStatusTextBlock;
            }

            set
            {
                if (value != loginStatusTextBlock)
                {
                    loginStatusTextBlock = value;
                    NotifyPropertyChanged("LoginStatusTextBlock");
                }
            }
        }

        private bool rememberMeIsChecked = false;

        public bool RememberMeIsChecked
        {
            get
            {
                return rememberMeIsChecked;
            }

            set
            {
                if (value != rememberMeIsChecked)
                {
                    rememberMeIsChecked = value;
                    NotifyPropertyChanged("RememberMeIsChecked");
                }
            }
        }

        private string tokenTextBox = "";

        public string TokenTextBox
        {
            get
            {
                return tokenTextBox;
            }

            set
            {
                if (value != tokenTextBox)
                {
                    tokenTextBox = value;
                    NotifyPropertyChanged("TokenTextBox");
                }
            }
        }
    }
}
