namespace PartyLooper.Models
{
    public class PlayerState
    {
        public bool IsPlaying { get; set; }

        public bool IsPaused { get; set; }

        public bool IsPartyMode { get; set; }

        public string CurrentFile { get; set; }

        public PlaylistItem CurrentPlaylistItem { get; set; }

        public string CurrentSong
        {
            get { 
                return System.IO.Path.GetFileNameWithoutExtension(CurrentFile); 
            }
        }
    }
}
