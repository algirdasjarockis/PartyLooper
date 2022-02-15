using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using MediaManager;
using PartyLooper.Services;
using PartyLooper.Models;

namespace PartyLooper.Views
{
    [QueryProperty(nameof(FilenameFromPlaylist), "filePath")]
    public partial class MainPage : ContentPage
    {
        private bool isSliderDragging = false;
        private PlayerState playerState = App.MainViewModel.PlayerState;
        private IMediaManager mediaPlayer = CrossMediaManager.Current;

        public IPlaylistStore<PlaylistItem> playlistStore => DependencyService.Get<IPlaylistStore<PlaylistItem>>();

        // setter used by Routing
        public string FilenameFromPlaylist
        {
            set 
            {
                playerState.CurrentPlaylistItem = App.PlaylistViewModel.SelectedPlaylistItem;
                PlayFile(value);

                sliderRangeControl.MaximumValue = playerState.CurrentPlaylistItem.TotalDuration;
                sliderRangeControl.LowerValue = playerState.CurrentPlaylistItem.LeftPosition;
                sliderRangeControl.UpperValue = playerState.CurrentPlaylistItem.RightPosition;
            }
        }

        public MainPage()
        {
            InitializeComponent();
            
            BindingContext = App.MainViewModel;

            btnOpenMedia.Clicked += OpenMediaDialog;
            btnAddToPlaylist.Clicked += AddCurrentSongToPlaylist;
            btnPlayPause.Clicked += (s, e) => PausePlaying();
            btnParty.Clicked += (s, e) =>
            {
                btnParty.Text = playerState.IsPartyMode ? "Party mode is ON" : "Party mode is OFF";
            };

            btnFixPositionLeft.Clicked += (s, e) => FixateRangeValue(true, sliderSongControl.Value);
            btnFixPositionRight.Clicked += (s, e) => FixateRangeValue(false, sliderSongControl.Value);

            sliderSongControl.DragCompleted += (s, e) => SeekMedia(sliderSongControl.Value);
            sliderSongControl.DragStarted += (s, e) => isSliderDragging = true;
            sliderSongControl.DragCompleted += (s, e) => isSliderDragging = false;

            sliderRangeControl.DragCompleted += (s, e) => SaveCurrentRanges();

            mediaPlayer.StateChanged += (s, e) =>
            {
                if (e.State == MediaManager.Player.MediaPlayerState.Playing)
                {
                    sliderSongControl.Maximum = mediaPlayer.Duration.TotalSeconds;
                    sliderRangeControl.MaximumValue = mediaPlayer.Duration.TotalSeconds;
                    btnPlayPause.Text = "Pause";
                    this.RunUiUpdateTimer();
                }
                else
                {
                    btnPlayPause.Text = "Play";
                }
            };
        }

        private async void OpenMediaDialog(object sender, EventArgs e)
        {
            await PickAndShow();
        }

        private async void AddCurrentSongToPlaylist(object sender, EventArgs e)
        {
            if (App.PlaylistViewModel.Exists(playerState.CurrentFile))
            {
                // file path already exists, nothing to do here
                return;
            }

            await addAndSavePlaylistItems();

            // update view
            lbPlaylistMsg.IsVisible = true;
            Device.StartTimer(TimeSpan.FromSeconds(5), () =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    lbPlaylistMsg.IsVisible = false;
                });

                return false;
            });
        }

        private async void SaveCurrentRanges()
        {
            PlaylistItem item = playerState.CurrentPlaylistItem;
            if (item != null)
            {
                item.LeftPosition = sliderRangeControl.LowerValue;
                item.RightPosition = sliderRangeControl.UpperValue;
                await playlistStore.PersistPlaylistAsync(App.PlaylistViewModel.PlaylistItems);
            }
        }

        private async Task addAndSavePlaylistItems()
        {
            App.PlaylistViewModel.PlaylistItems.Add(new PlaylistItem()
            {
                SongName = Path.GetFileNameWithoutExtension(playerState.CurrentFile),
                FilePath = playerState.CurrentFile,
                LeftPosition = sliderRangeControl.LowerValue,
                RightPosition = sliderRangeControl.UpperValue,
                TotalDuration = mediaPlayer.Duration.TotalSeconds
            });

            await this.playlistStore.PersistPlaylistAsync(App.PlaylistViewModel.PlaylistItems);
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
                    playerState.CurrentPlaylistItem = null;
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
            playerState.CurrentFile = filePath;

            btnPlayPause.IsEnabled = btnAddToPlaylist.IsEnabled = true;
            await mediaPlayer.Play(filePath);
        }

        void RunUiUpdateTimer()
        {
            Device.StartTimer(TimeSpan.FromMilliseconds(100), () =>
            {
                if (!mediaPlayer.IsPlaying())
                {
                    return false;
                }

                Device.BeginInvokeOnMainThread(() =>
                {
                    if (playerState.IsPartyMode)
                    {
                        var pos = mediaPlayer.Position.TotalSeconds;
                        if (pos >= sliderRangeControl.UpperValue || pos < sliderRangeControl.LowerValue)
                        {
                            SeekMedia(sliderRangeControl.LowerValue);
                        }
                    }

                    var t = mediaPlayer.Duration - mediaPlayer.Position;

                    var timeLeft = string.Format(
                        "{0:D2}:{1:D2}:{2:D3}",
                        t.Minutes,
                        t.Seconds,
                        t.Milliseconds
                    );

                    lbStatus.Text = timeLeft;
                    lbPlayingTime.Text = mediaPlayer.Position.TotalSeconds.ToString();

                    if (!isSliderDragging)
                    {
                        sliderSongControl.Value = mediaPlayer.Position.TotalSeconds;
                    }
                });

                return true;
            });
        }

        async void PausePlaying()
        {
            await CrossMediaManager.Current.PlayPause();
        }

        async void SeekMedia(double seconds)
        {
            await mediaPlayer.SeekTo(TimeSpan.FromSeconds(seconds));
        }

        void FixateRangeValue(bool isLeft, double position)
        {
            if (isLeft)
            {
                sliderRangeControl.LowerValue = position;
                if (position > sliderRangeControl.UpperValue)
                {
                    sliderRangeControl.UpperValue = position >= sliderRangeControl.MaximumValue 
                        ? position
                        : position + 1;
                }
            } else
            {
                sliderRangeControl.UpperValue = position;
                if (position < sliderRangeControl.LowerValue)
                {
                    sliderRangeControl.LowerValue = position > 0 ? position - 1 : 0;
                }
            }

            SaveCurrentRanges();
        }
    }
}