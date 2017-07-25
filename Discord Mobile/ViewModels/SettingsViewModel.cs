using Discord_Mobile.Services;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Discord_Mobile.ViewModels
{
    class SettingsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

        public void Initialize(object sender, RoutedEventArgs e)
        {
            EditedUsername = LoginService.client.CurrentUser.Username;
        }

        public async void Disconnect()
        {
            ContentDialog DisconnectDialog = new ContentDialog
            {
                Title = "Are you sure ?",
                Content = "Be aware, the saved token will be lost!",
                PrimaryButtonText = "Disconnect",
                CloseButtonText = "It was an accident",
                DefaultButton = ContentDialogButton.Close
            };
            ContentDialogResult result = await DisconnectDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                LoginService.DeleteUser();

                await LoginService.client.LogoutAsync();

                Frame rootFrame = Window.Current.Content as Frame;
                rootFrame.Navigate(typeof(LoginView));
            }
        }
        
        public void EnableEditUsernameTextbox(object sender, RoutedEventArgs e)
        {
            IsUsernameEditable = true;
        }

        public async void EditUser(object sender, RoutedEventArgs e)
        {
            await LoginService.client.CurrentUser.ModifyAsync(selfUserProperties => { selfUserProperties.Username = EditedUsername; });
        }

        //#########################################################################################

        public string Version
        {
            get
            {
                return string.Format("{0}.{1}.{2}.{3}",
                    Package.Current.Id.Version.Major,
                    Package.Current.Id.Version.Minor,
                    Package.Current.Id.Version.Build,
                    Package.Current.Id.Version.Revision);
            }
        }

        private bool isUsernameEditable = false;

        public bool IsUsernameEditable
        {
            get
            {
                return isUsernameEditable;
            }
            set
            {
                if (value != isUsernameEditable)
                {
                    isUsernameEditable = value;
                    NotifyPropertyChanged("IsUsernameEditable");
                }
            }
        }

        private string editedUsername = "";

        public string EditedUsername
        {
            get
            {
                return editedUsername;
            }
            set
            {
                if (value != editedUsername)
                {
                    editedUsername = value;
                    NotifyPropertyChanged("EditedUsername");
                }
            }
        }

        public bool EnableSounds
        {
            get
            {
                if (localSettings.Values["EnableSounds"] == null)
                {
                    localSettings.Values["EnableSounds"] = true;
                    return true;
                }
                return (bool)localSettings.Values["EnableSounds"];
            }
            set
            {
                if (value != (bool)localSettings.Values["EnableSounds"])
                {
                    localSettings.Values["EnableSounds"] = value;
                    NotifyPropertyChanged("EnableSounds");
                }
            }
        }
    }
}
