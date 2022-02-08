using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using MediaManager;

namespace PartyLooper.Views
{
    public partial class MainPage : ContentPage
    {
        string selectedFilePath;

        bool isSliderDragging = false;
        bool isPartyMode = false;
        IMediaManager mediaPlayer = CrossMediaManager.Current;

        public MainPage()
        {
            InitializeComponent();
            BindingContext = App.MainViewModel;        

            btnOpenMedia.Clicked += OpenMediaDialog;
            btnPlay.Clicked += (s, e) => PlayCurrentFile();
            btnPause.Clicked += (s, e) => PausePlaying();
            btnStop.Clicked += (s, e) => StopPlaying();
            btnParty.Clicked += (s, e) =>
            {
                isPartyMode = !isPartyMode;
                btnParty.Text = isPartyMode ? "Party mode is ON" : "Party mode is OFF";
            };

            btnFixPositionLeft.Clicked += (s, e) => FixateRangeValue(true, sliderSongControl.Value);
            btnFixPositionRight.Clicked += (s, e) => FixateRangeValue(false, sliderSongControl.Value);

            sliderSongControl.DragCompleted += (s, e) => SeekMedia(sliderSongControl.Value);
            sliderSongControl.DragStarted += (s, e) => isSliderDragging = true;
            sliderSongControl.DragCompleted += (s, e) => isSliderDragging = false;

            //sliderSongControl.LowerDragCompleted += (s, e) => SeekMedia(sliderSongControl.LowerValue);

            CrossMediaManager.Current.StateChanged += (s, e) =>
            {
                if (e.State == MediaManager.Player.MediaPlayerState.Playing)
                {
                    sliderSongControl.Maximum = mediaPlayer.Duration.TotalSeconds;
                    sliderRangeControl.MaximumValue = mediaPlayer.Duration.TotalSeconds;
                    btnPause.Text = "||";
                }
                else
                {
                    btnPause.Text = ">";
                }
            };
        }

        private async void OpenMediaDialog(object sender, EventArgs e)
        {
            await this.PickAndShow();
        }

        async Task<FileResult> PickAndShow()
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
                if (result != null)
                {
                    //var stream = await result.OpenReadAsync();
                    //Image = ImageSource.FromStream(() => stream);

                    if (result.FileName.EndsWith(".mp3"))
                    {
                        this.selectedFilePath = result.FullPath;
                        lbStatus.Text = result.FullPath;
                    }
                 }

                return result;
            }
            catch (Exception ex)
            {
                // The user canceled or something went wrong
            }

            return null;
        }

        async void PlayCurrentFile()
        {
            if (this.selectedFilePath != null)
            {
                await mediaPlayer.Play(this.selectedFilePath);

                Device.StartTimer(TimeSpan.FromMilliseconds(100), () =>
                {
                    if (isSliderDragging)
                    {
                        return true;
                    }

                    if (!mediaPlayer.IsPlaying())
                    {
                        return false;
                    }

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        if (isPartyMode)
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

                        sliderSongControl.Value = mediaPlayer.Position.TotalSeconds;
                    });

                    return true;
                });
            }
        }

        async void StopPlaying()
        {
            if (CrossMediaManager.Current.IsPlaying())
            {
                await CrossMediaManager.Current.Stop();
            }
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
        }
    }
}