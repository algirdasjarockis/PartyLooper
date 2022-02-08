using PartyLooper.Services;
using PartyLooper.Views;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using PartyLooper.ViewModels;

namespace PartyLooper
{
    public partial class App : Application
    {
        public static MainViewModel MainViewModel { get; private set; }
        public App()
        {
            InitializeComponent();
            MainViewModel = new MainViewModel();

            DependencyService.Register<MockDataStore>();
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
