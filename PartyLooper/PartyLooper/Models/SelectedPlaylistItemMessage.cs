using CommunityToolkit.Mvvm.Messaging.Messages;

namespace PartyLooper.Models
{
    public class SelectedPlaylistItemMessage : AsyncRequestMessage<PlaylistItem>
    {
        public PlaylistItem SelectedPlaylistItem { get; }
        public SelectedPlaylistItemMessage(PlaylistItem item)
        {
            SelectedPlaylistItem = item;
        }
    }
}
