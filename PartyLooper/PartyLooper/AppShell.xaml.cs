using PartyLooper.ViewModels;
using PartyLooper.Views;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace PartyLooper
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(ItemDetailPage), typeof(ItemDetailPage));
            Routing.RegisterRoute(nameof(NewItemPage), typeof(NewItemPage));
            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
        }
    }
}
