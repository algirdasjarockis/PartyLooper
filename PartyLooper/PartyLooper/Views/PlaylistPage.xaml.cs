using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using PartyLooper.ViewModels;
using PartyLooper.Models;

namespace PartyLooper.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PlaylistPage : ContentPage
    {
        public PlaylistPage()
        {
            InitializeComponent();

            BindingContext = App.PlaylistViewModel;
            this.viewPlaylistItems.SelectionChanged += OnPlaylistItemSelect;
        }

        private void OnPlaylistItemSelect(object sender, SelectionChangedEventArgs e)
        {
            string songPath = (e.CurrentSelection.FirstOrDefault() as PlaylistItem)?.FilePath;
            System.Console.WriteLine(songPath);
            Shell.Current.GoToAsync("//" + nameof(MainPage) + $"?filePath={songPath}");
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            App.PlaylistViewModel.OnAppearing();
        }
    }
}