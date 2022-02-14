using PartyLooper.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

namespace PartyLooper.Services
{
    public class PlaylistStore : IPlaylistStore<PlaylistItem>
    {
        private string playlistFile;
        public PlaylistStore()
        {
            this.playlistFile = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "playlist.txt"
            );
        }

        public async Task<IEnumerable<PlaylistItem>> LoadPlaylistAsync()
        {
            // /data/user/0/com.companyname.partylooper/files/.config/playlist.txt
            List<PlaylistItem> items = new List<PlaylistItem>();
            if (!File.Exists(this.playlistFile))
            {
                return items;
            }

            StreamReader sr = new StreamReader(new FileStream(this.playlistFile, FileMode.Open, FileAccess.Read, FileShare.Read));
            string line;

            while ((line = await sr.ReadLineAsync()) != null)
            {
                items.Add(new PlaylistItem()
                {
                    SongName = Path.GetFileNameWithoutExtension(line),
                    FilePath = line
                });
            }

            sr.Close();

            return await Task.FromResult(items);
        }

        public async Task PersistPlaylistAsync(IEnumerable<PlaylistItem> items)
        {
            FileStream fs = new FileStream(this.playlistFile, FileMode.Create, FileAccess.Write, FileShare.Read);

            StreamWriter writer = new StreamWriter(fs);
            foreach (var item in App.PlaylistViewModel.PlaylistItems)
            {
                await writer.WriteLineAsync(item.FilePath);
            }

            writer.Flush();
            writer.Close();
        }
    }
}