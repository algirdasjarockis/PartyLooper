using PartyLooper.Services;
using PartyLooper.Views;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using PartyLooper.ViewModels;
using PartyLooper.Models;
using MediaManager;

namespace PartyLooper
{
    public partial class App : Application
    {
        public static PlaylistViewModel PlaylistViewModel { get; private set; }
        public static PlayerViewModel PlayerViewModel { get; private set; }
        public App()
        {
            InitializeComponent();

            DependencyService.Register<PlaylistStore>();

            PlaylistViewModel = new PlaylistViewModel();
            PlayerViewModel = new PlayerViewModel(CrossMediaManager.Current, DependencyService.Get<IPlaylistStore<PlaylistItem>>());

            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
