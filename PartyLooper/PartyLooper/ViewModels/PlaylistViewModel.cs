using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using PartyLooper.Models;
using PartyLooper.Services;
using Xamarin.Forms;

namespace PartyLooper.ViewModels
{
    public class PlaylistViewModel : BaseViewModel
    {
        public Collection<PlaylistItem> PlaylistItems { get; }
        private IPlaylistStore<PlaylistItem> playlistStore => DependencyService.Get<IPlaylistStore<PlaylistItem>>();

        public Command LoadItemsCommand { get; }

        public PlaylistViewModel()
        {
            PlaylistItems = new Collection<PlaylistItem>();
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());
        } 

        async Task ExecuteLoadItemsCommand()
        {
            IsBusy = true;

            this.PlaylistItems.Clear();
            var items = await this.playlistStore.LoadPlaylistAsync();

            foreach (var item in items)
            {
                this.PlaylistItems.Add(item);
            }

            IsBusy = false;
        }

        public bool Exists(string filename)
        {
            foreach (var item in this.PlaylistItems)
            {
                if (item.FilePath == filename) { return true; }
            }

            return false;
        }

        public void OnAppearing()
        {
            IsBusy = true;
        }
    }
}
