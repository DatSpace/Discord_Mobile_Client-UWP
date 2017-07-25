using Discord_Mobile.Services;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
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

        LoginService loginService = new LoginService();

        public async Task AutoLogin()
        {
            PasswordCredential loginCredential = LoginService.GetCredit();

            //If there is a credential stored
            if (loginCredential != null)
            {
                LoginUIVisibility = Visibility.Collapsed;
                //Get the password into the credential object.
                loginCredential.RetrievePassword();
                //And try to connect!
                try
                {
                    NetworkConnectivityLevel connectionStatus = NetworkInformation.GetInternetConnectionProfile().GetNetworkConnectivityLevel();
                    switch (connectionStatus)
                    {
                        case NetworkConnectivityLevel.InternetAccess:
                            await loginService.MakeConnectionAsync(loginCredential.Password);
                            Frame rootFrame = Window.Current.Content as Frame;
                            rootFrame.Navigate(typeof(ChatView));
                            break;
                        case NetworkConnectivityLevel.ConstrainedInternetAccess:
                            LoginStatusTextBlock = "Limited Internet Access";
                            break;
                        default:
                            LoginStatusTextBlock = "No Internet Acccess";
                            break;
                    }
                }
                catch
                {
                    LoginStatusTextBlock = "Can't connect at the moment. Sorry :/";
                }
            }
        }

        public async void ManualLogin()
        {
            // There is no credential stored in the locker.
            try
            {
                NetworkConnectivityLevel connectionStatus = NetworkInformation.GetInternetConnectionProfile().GetNetworkConnectivityLevel();
                switch (connectionStatus)
                {
                    case NetworkConnectivityLevel.InternetAccess:
                        LoginButtonIsEnabled = false;
                        await loginService.MakeConnectionAsync(TokenTextBox);
                        if (RememberMeIsChecked)
                            LoginService.SaveUser(TokenTextBox);
                        Frame rootFrame = Window.Current.Content as Frame;
                        rootFrame.Navigate(typeof(ChatView));
                        break;
                    case NetworkConnectivityLevel.ConstrainedInternetAccess:
                        ConnectionErrorTextBlock = "Limited Internet Access!";
                        break;
                    default:
                        ConnectionErrorTextBlock = "No Internet Connection!";
                        break;
                }
            }
            catch
            {
                LoginButtonIsEnabled = true;
                ConnectionErrorTextBlock = "Error! Please re-check the token!";
            }

        }

        //#################< START OF PROPERTIES >######################

        private Visibility loginUIVisibility = Visibility.Visible;
        public Visibility ConnectingVisibility = Visibility.Collapsed;

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
                    ConnectingVisibility = Visibility.Collapsed;
                    if (loginUIVisibility == Visibility.Collapsed)
                        ConnectingVisibility = Visibility.Visible;
                    NotifyPropertyChanged("LoginUIVisibility");
                    NotifyPropertyChanged("ConnectingVisibility");
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
