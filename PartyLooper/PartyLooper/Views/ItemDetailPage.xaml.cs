using PartyLooper.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace PartyLooper.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}