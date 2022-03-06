using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using System.IO;
using MediaManager;
using MediaManager.Playback;
using Xamarin.Forms;
using Xamarin.Essentials;
using CommunityToolkit.Mvvm.Messaging;
using PartyLooper.Models;
using PartyLooper.Services;

namespace PartyLooper.ViewModels
{
    public class PlayerViewModel : BaseViewModel
    {
        private Timer timerPlaylistSave;
        private bool currentlyDragging = false;
        private double positionRequest;
        private string buttonTextPlayPause;
        private string buttonTextParty;
        private bool allowedToAddToPlaylist = false;
        private string playlistMsg;
        private string timeLeft = "";
        private string playingTime = "";
        private double segmentLowerValue = 0;
        private double segmentUpperValue = 100;
        private double segmentMaxValue = 100;
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
            { SetProperty(ref buttonTextPlayPause, value); }
        }
        public string ButtonTextParty
        {
            get => buttonTextParty;
            set { SetProperty(ref buttonTextParty, value); }
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

        public string TimeLeft
        {
            get => timeLeft;
            set { SetProperty(ref timeLeft, value); }
        }

        public string PlayingTime
        {
            get => playingTime;
            set { SetProperty(ref playingTime, value); }
        }

        public double SegmentLowerValue
        {
            get => segmentLowerValue;
            set { SetProperty(ref segmentLowerValue, value); }
        }

        public double SegmentUpperValue
        {
            get => segmentUpperValue;
            set { SetProperty(ref segmentUpperValue, value); }
        }

        public double SegmentMaxValue
        {
            get => segmentMaxValue;
            set { SetProperty(ref segmentMaxValue, value); }
        }

        public double Duration { get; private set; }

        public Command PlayCommand { get; }
        public Command DragCompletedCommand { get; }
        public Command DragStartedCommand { get; }
        public Command SegmentSelectDragCompletedCommand { get; }
        public Command PartyCommand { get; }
        public Command OpenMediaCommand { get; }
        public Command AddToPlaylistCommand { get; }
        public Command<string> FixateRangeCommand { get; }
        public Command<string> TuneRangeCommand { get; }

        public PlayerViewModel(IMediaManager mediaManager, IPlaylistStore<PlaylistItem> playlistStore)
        {
            this.mediaManager = mediaManager;
            this.playlistStore = playlistStore;
           
            // texts
            ButtonTextPlayPause = "Play";
            ButtonTextParty = "Party!";
            Title = "Welcome to PartyLooper";

            // initial values
            Duration = 100;
            AllowedToAddToPlaylist = false;
            PlayerState = new PlayerState()
            {
                IsPartyMode = false,
                IsPaused = true,
                IsPlaying = false
            };

            // UI element commands
            PartyCommand = new Command(OnParty);
            OpenMediaCommand = new Command(async () => await PickAndShow());
            PlayCommand = new Command(() => this.mediaManager.PlayPause());
            DragCompletedCommand = new Command(DragCompleted);
            SegmentSelectDragCompletedCommand = new Command(SaveCurrentRanges);
            DragStartedCommand = new Command(() => currentlyDragging = true);
            AddToPlaylistCommand = new Command(AddCurrentSongToPlaylist);
            FixateRangeCommand = new Command<string>(FixateRangeValue);
            TuneRangeCommand = new Command<string>(TuneRangeValue);

            // media player events
            this.mediaManager.PositionChanged += PlayerPositionChanged;
            this.mediaManager.StateChanged += PlayerStateChanged;

            // subscribe for playlist item selection
            WeakReferenceMessenger.Default.Register<SelectedPlaylistItemMessage>(this, (r, m) => PlayPlaylistItem(m.SelectedPlaylistItem));

            // timer for delayed playlist saving
            timerPlaylistSave = new Timer(3000);
            timerPlaylistSave.Elapsed += (s, e) => { SaveCurrentRanges(); };
            timerPlaylistSave.AutoReset = false;
            timerPlaylistSave.Enabled = false;
        }

        public void DragCompleted()
        {
            currentlyDragging = false;
            this.mediaManager.SeekTo(TimeSpan.FromSeconds(PositionRequest));
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
                Duration = mediaManager.Duration.TotalSeconds;
                SegmentMaxValue = mediaManager.Duration.TotalMilliseconds;
                ButtonTextPlayPause = "Pause";

                OnPropertyChanged(nameof(Duration));
                RunUiUpdateTimer();
            }
            else if (e.State == MediaManager.Player.MediaPlayerState.Paused)
            {
                ButtonTextPlayPause = "Play";
            }
        }

        private void OnParty()
        {
            PlayerState.IsPartyMode = !PlayerState.IsPartyMode;
            ButtonTextParty = "Party Mode: " + (PlayerState.IsPartyMode ? "ON" : "OFF");
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

        async void PlayPlaylistItem(PlaylistItem item)
        {
            PlayerState.CurrentPlaylistItem = item;
            PlayFile(item.FilePath);

            SegmentMaxValue = item.TotalDuration;
            SegmentLowerValue = item.LeftPosition;
            SegmentUpperValue = item.RightPosition;

            await Shell.Current.GoToAsync("//player");
        }

        private async void AddCurrentSongToPlaylist()
        {
            if (PlayerState.CurrentFile == null || App.PlaylistViewModel.Exists(PlayerState.CurrentFile))
            {
                // file path already exists, nothing to do here
                return;
            }

            if (!App.PlaylistViewModel.IsLoaded)
            {
                await App.PlaylistViewModel.ExecuteLoadItemsCommand();
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
                LeftPosition = SegmentLowerValue,
                RightPosition = SegmentUpperValue,
                TotalDuration = SegmentMaxValue
            });

            await playlistStore.PersistPlaylistAsync(App.PlaylistViewModel.PlaylistItems);
            AllowedToAddToPlaylist = false;
        }

        void RunUiUpdateTimer()
        {
            Device.StartTimer(TimeSpan.FromMilliseconds(100), () =>
            {
                if (!mediaManager.IsPlaying())
                {
                    return false;
                }

                Device.BeginInvokeOnMainThread(() =>
                {
                    if (PlayerState.IsPartyMode)
                    {
                        var pos = mediaManager.Position.TotalMilliseconds;
                        if (pos >= SegmentUpperValue || pos < SegmentLowerValue)
                        {
                            mediaManager.SeekTo(TimeSpan.FromMilliseconds(SegmentLowerValue));
                        }
                    }

                    var t = mediaManager.Duration - mediaManager.Position;

                    TimeLeft = string.Format(
                        "{0:D2}:{1:D2}:{2:D3}",
                        t.Minutes,
                        t.Seconds,
                        t.Milliseconds
                    );

                    PlayingTime = mediaManager.Position.TotalSeconds.ToString();
                });

                return true;
            });
        }

        private async void SaveCurrentRanges()
        {
            PlaylistItem item = PlayerState.CurrentPlaylistItem;
            if (item != null)
            {
                item.LeftPosition = SegmentLowerValue;
                item.RightPosition = SegmentUpperValue;
                item.TotalDuration = SegmentMaxValue;
                await playlistStore.PersistPlaylistAsync(App.PlaylistViewModel.PlaylistItems);
            }
        }

        void FixateRangeValue(string direction)
        {
            var position = Position * 1000;
            if (direction == "L")
            {
                SegmentLowerValue = position;
                if (position > SegmentUpperValue)
                {
                    SegmentUpperValue = position >= SegmentMaxValue
                        ? position
                        : position + 1;
                }
            }
            else
            {
                SegmentUpperValue = position;
                if (position < SegmentLowerValue)
                {
                    SegmentLowerValue = position > 0 ? position - 1 : 0;
                }
            }

            SchedulePlaylistSaving();
        }

        //
        // direction consists of 2 chars:
        //      direction[0] - L or R, left or right handle is being used
        //      direction[1] - L or R, to which direction range should be adjusted
        //
        void TuneRangeValue(string direction)
        {
            if (direction.Length != 2) { return; }
            var dir = direction[1] == 'L' ? -1 : 1;

            double step = 200 * dir;
            if (direction[0] == 'L')
            {
                SegmentLowerValue += step;
                if (PlayerState.IsPartyMode)
                {
                    mediaManager.SeekTo(TimeSpan.FromMilliseconds(SegmentLowerValue));
                }
            }
            else
            {
                SegmentUpperValue += step;
            }

            SchedulePlaylistSaving();
        }

        void SchedulePlaylistSaving()
        {
            if (!timerPlaylistSave.Enabled)
            {
                timerPlaylistSave.Enabled = true;
            }
        }
    }
}
