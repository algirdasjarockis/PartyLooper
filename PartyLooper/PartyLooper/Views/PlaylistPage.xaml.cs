using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PartyLooper.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PlaylistPage : ContentPage
    {
        public PlaylistPage()
        {
            InitializeComponent();

            BindingContext = App.PlaylistViewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            App.PlaylistViewModel.OnAppearing();
        }
    }
}