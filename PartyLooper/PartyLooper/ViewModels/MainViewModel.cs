using System;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace PartyLooper.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private bool allowedToAddToPlaylist;

        public Command PartyCommand { get; }

        public MainViewModel()
        {
            AllowedToAddToPlaylist = false;
            Title = "Welcome to PartyLooper";
            PlayerState = new Models.PlayerState()
            {
                IsPartyMode = false,
                IsPaused = true,
                IsPlaying = false
            };

            PartyCommand = new Command(OnParty);
        }

        public bool AllowedToAddToPlaylist
        {
            get => allowedToAddToPlaylist; 
            set
            {
                SetProperty(ref allowedToAddToPlaylist, value);
            }
        }

        private void OnParty()
        {
            PlayerState.IsPartyMode = !PlayerState.IsPartyMode;
        }
    }
}