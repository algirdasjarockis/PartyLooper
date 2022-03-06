using Xamarin.Forms;

namespace PartyLooper.Views
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            
            BindingContext = App.PlayerViewModel;
        }
    }
}