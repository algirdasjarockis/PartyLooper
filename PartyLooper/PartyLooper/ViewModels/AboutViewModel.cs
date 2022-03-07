using System;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace PartyLooper.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        public AboutViewModel()
        {
            Title = "About PartyLooper";
            OpenWebCommand = new Command(async () => await Browser.OpenAsync("https://github.com/algirdasjarockis/PartyLooper"));
        }

        public ICommand OpenWebCommand { get; }
    }
}