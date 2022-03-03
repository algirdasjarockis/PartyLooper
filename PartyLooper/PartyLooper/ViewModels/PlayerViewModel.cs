using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using MediaManager;
using MediaManager.Playback;
using Xamarin.Forms;
using Xamarin.Essentials;
using PartyLooper.Models;
using PartyLooper.Services;

namespace PartyLooper.ViewModels
{
    public class PlayerViewModel : BaseViewModel
    {
        private bool currentlyDragging = false;
        private double positionRequest;
        private string buttonTextPlayPause;
        private bool allowedToAddToPlaylist;
        private string playlistMsg;
        private readonly IMediaManager mediaManager;
        private readonly IPlaylistStore<PlaylistItem> playlistStore;

        public bool AllowedToAddToPlaylist
        {
            get => allowedToAddToPlaylist;
            set
            {
                SetProperty(ref allowedToAddToPlaylist, value);
            }
        }

        public string ButtonTextPlayPause
        {
            get => buttonTextPlayPause;
            set
            {
                SetProperty(ref buttonTextPlayPause, value);
            }
        }

        public double Position
        {
            get => mediaManager.Position.TotalSeconds;
            set 
            {
                positionRequest = value;
            }
        }

        public double PositionRequest
        {
            get => positionRequest;
            set
            {
                positionRequest = value;
            }
        }

        public string PlaylistMessage
        {
            get => playlistMsg;
            set
            {
                SetProperty(ref playlistMsg, value);
            }
        }

        public double Duration { get; private set; }

        public Command PlayCommand { get; }
        public Command DragCompletedCommand { get; }
        public Command DragStartedCommand { get; }
        public Command PartyCommand { get; }
        public Command OpenMediaCommand { get; }

        public Command AddToPlaylistCommand { get; }

        public PlayerViewModel(IMediaManager mediaManager, IPlaylistStore<PlaylistItem> playlistStore)
        {
            this.mediaManager = mediaManager;
            this.playlistStore = playlistStore;
           
            Duration = 100;
            ButtonTextPlayPause = "Play";
            AllowedToAddToPlaylist = false;
            Title = "Welcome to PartyLooper";
            PlayerState = new Models.PlayerState()
            {
                IsPartyMode = false,
                IsPaused = true,
                IsPlaying = false
            };

            PartyCommand = new Command(OnParty);
            OpenMediaCommand = new Command(async () => await PickAndShow());

            PlayCommand = new Command(() => this.mediaManager.PlayPause());
            DragCompletedCommand = new Command(() => 
            {
                currentlyDragging = false;
                this.mediaManager.SeekTo(TimeSpan.FromSeconds(positionRequest));
            });
            DragStartedCommand = new Command(() => currentlyDragging = true);
            AddToPlaylistCommand = new Command(AddCurrentSongToPlaylist);

            this.mediaManager.PositionChanged += PlayerPositionChanged;
            this.mediaManager.StateChanged += PlayerStateChanged;
        }

        private void PlayerPositionChanged(object sender, MediaManager.Playback.PositionChangedEventArgs e)
        {
            if (!currentlyDragging)
            {
                OnPropertyChanged(nameof(Position));
            }
        }

        private void PlayerStateChanged(object sender, StateChangedEventArgs e)
        {
            if (e.State == MediaManager.Player.MediaPlayerState.Playing)
            {
                Duration = this.mediaManager.Duration.TotalSeconds;
                ButtonTextPlayPause = "Pause";

                OnPropertyChanged(nameof(Duration));
            }
            else if (e.State == MediaManager.Player.MediaPlayerState.Paused)
            {
                ButtonTextPlayPause = "Play";
            }
        }

        private void OnParty()
        {
            PlayerState.IsPartyMode = !PlayerState.IsPartyMode;
        }

        private async Task<FileResult> PickAndShow()
        {
            var customFileType =
                new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.iOS, new[] { "audio/mpeg" } },
                    { DevicePlatform.Android, new[] { "audio/mpeg", "image/jpeg" } },
                });

            var options = new PickOptions
            {
                PickerTitle = "Please select an audio file",
                FileTypes = customFileType,
            };

            try
            {
                var result = await FilePicker.PickAsync(options);
                if (result != null && result.FileName.EndsWith(".mp3"))
                {
                    PlayerState.CurrentPlaylistItem = null;
                    PlayFile(result.FullPath);
                }

                return result;
            }
            catch (Exception ex)
            {
                // The user canceled or something went wrong
            }

            return null;
        }

        async void PlayFile(string filePath)
        {
            PlayerState.CurrentFile = filePath;
            AllowedToAddToPlaylist = !App.PlaylistViewModel.Exists(filePath);

            await mediaManager.Play(filePath);
        }

        private async void AddCurrentSongToPlaylist()
        {
            if (App.PlaylistViewModel.Exists(PlayerState.CurrentFile))
            {
                // file path already exists, nothing to do here
                return;
            }

            await addAndSavePlaylistItems();

            // update view
            PlaylistMessage = "Current song added to the playlist";
            Device.StartTimer(TimeSpan.FromSeconds(5), () =>
            {
                PlaylistMessage = "";
                return false;
            });
        }

        private async Task addAndSavePlaylistItems()
        {
            App.PlaylistViewModel.PlaylistItems.Add(new PlaylistItem()
            {
                SongName = Path.GetFileNameWithoutExtension(PlayerState.CurrentFile),
                FilePath = PlayerState.CurrentFile,
                //LeftPosition = sliderRangeControl.LowerValue,
                //RightPosition = sliderRangeControl.UpperValue,
                TotalDuration = mediaManager.Duration.TotalMilliseconds
            });

            await playlistStore.PersistPlaylistAsync(App.PlaylistViewModel.PlaylistItems);
            AllowedToAddToPlaylist = false;
        }
    }
}
