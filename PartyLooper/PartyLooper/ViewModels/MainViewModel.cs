using System;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace PartyLooper.ViewModels
{
    public class MainViewModel : BaseViewModel
    { 
        public Command PartyCommand { get; }

        public MainViewModel()
        {
            Title = "Welcome to PartyLooper";
            PlayerState = new Models.PlayerState()
            {
                IsPartyMode = false,
                IsPaused = true,
                IsPlaying = false
            };

            PartyCommand = new Command(OnParty);
        }

        private void OnParty()
        {
            PlayerState.IsPartyMode = !PlayerState.IsPartyMode;
        }
    }
}