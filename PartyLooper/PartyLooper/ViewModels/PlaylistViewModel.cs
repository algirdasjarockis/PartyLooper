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
        public ObservableCollection<PlaylistItem> PlaylistItems { get; }
        private IPlaylistStore<PlaylistItem> playlistStore => DependencyService.Get<IPlaylistStore<PlaylistItem>>();

        public Command LoadItemsCommand { get; }
        public Command<PlaylistItem> RemoveItemCommand { get; }

        public PlaylistViewModel()
        {
            PlaylistItems = new ObservableCollection<PlaylistItem>();
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());
            RemoveItemCommand = new Command<PlaylistItem>(OnRemoveItemClick);
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

        async void OnRemoveItemClick(PlaylistItem item)
        {
            this.PlaylistItems.Remove(item);
            await this.playlistStore.PersistPlaylistAsync(this.PlaylistItems);
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
