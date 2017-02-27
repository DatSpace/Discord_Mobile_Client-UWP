﻿using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Discord_Mobile.ViewModels;
using System.Threading.Tasks;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Discord_Mobile
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ChatPage : Page
    {
        ChatViewModel chatViewModel = new ChatViewModel();

        public ChatPage()
        {
            this.InitializeComponent();
        }
        private void OpenSplitView(object sender, RoutedEventArgs e)
        {
            ChannelsSplitView.IsPaneOpen = !ChannelsSplitView.IsPaneOpen;
        }
    }
}
