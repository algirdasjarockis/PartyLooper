using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using PartyLooper.Models;
using PartyLooper.Services;
using Xamarin.Forms;
using CommunityToolkit.Mvvm.Messaging;

namespace PartyLooper.ViewModels
{
    public class PlaylistViewModel : BaseViewModel
    {
        public bool IsLoaded { get; private set; }
        public ObservableCollection<PlaylistItem> PlaylistItems { get; }
        public PlaylistItem SelectedPlaylistItem { set; get; }

        private IPlaylistStore<PlaylistItem> playlistStore => DependencyService.Get<IPlaylistStore<PlaylistItem>>();

        public Command LoadItemsCommand { get; }
        public Command<PlaylistItem> RemoveItemCommand { get; }
        public Command<PlaylistItem> SelectionChangedCommand { get; }

        public PlaylistViewModel()
        {
            IsLoaded = false;
            PlaylistItems = new ObservableCollection<PlaylistItem>();
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());
            RemoveItemCommand = new Command<PlaylistItem>(OnRemoveItemClick);
            SelectionChangedCommand = new Command<PlaylistItem>(OnPlaylistItemSelect);
        } 

        public async Task ExecuteLoadItemsCommand()
        {
            IsBusy = true;

            this.PlaylistItems.Clear();
            var items = await this.playlistStore.LoadPlaylistAsync();

            foreach (var item in items)
            {
                this.PlaylistItems.Add(item);
            }

            IsBusy = false;
            IsLoaded = true;
        }

        async void OnRemoveItemClick(PlaylistItem item)
        {
            this.PlaylistItems.Remove(item);
            await this.playlistStore.PersistPlaylistAsync(this.PlaylistItems);
        }

        private void OnPlaylistItemSelect(PlaylistItem currentItem)
        {
            //PlaylistItem currentItem = (e.CurrentSelection.FirstOrDefault() as PlaylistItem);

            SelectedPlaylistItem = currentItem;

            WeakReferenceMessenger.Default.Send(new SelectedPlaylistItemMessage(currentItem));
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
