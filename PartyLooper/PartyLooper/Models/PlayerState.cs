using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.IO;

namespace PartyLooper.Models
{
    public class PlayerState : INotifyPropertyChanged
    {
        private string currentFile;
        private PlaylistItem playlistItem;

        public bool IsPlaying { get; set; }

        public bool IsPaused { get; set; }

        public bool IsPartyMode { get; set; }

        public string CurrentFile { 
            get
            {
                return currentFile;
            }
            set
            {
                currentFile = value;
                this.CurrentSong = Path.GetFileNameWithoutExtension(value);
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(CurrentSong));
            }
        }

        public PlaylistItem CurrentPlaylistItem {
            get
            {
                return playlistItem;
            }
            set
            {
                playlistItem = value;
                NotifyPropertyChanged();
            } 
        }

        public string CurrentSong { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
