using PartyLooper.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

namespace PartyLooper.Services
{
    public class PlaylistStore : IPlaylistStore<PlaylistItem>
    {
        const string PlaylistFileName = "data.csv"; // playlist.txt previously
        const char DataSeparator = ';';

        private string playlistFile;

        public PlaylistStore()
        {
            this.playlistFile = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                PlaylistFileName
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
                var parts = line.Split(DataSeparator);

                items.Add(new PlaylistItem()
                {
                    SongName = Path.GetFileNameWithoutExtension(parts[0]),
                    FilePath = parts[0],
                    LeftPosition = double.Parse(parts[1]),
                    RightPosition = double.Parse(parts[2]),
                    TotalDuration = double.Parse(parts[3])
                });
            }

            sr.Close();

            return await Task.FromResult(items);
        }

        public async Task PersistPlaylistAsync(IEnumerable<PlaylistItem> items)
        {
            System.Console.WriteLine($"Persisting playlist");
            FileStream fs = new FileStream(this.playlistFile, FileMode.Create, FileAccess.Write, FileShare.Read);

            StreamWriter writer = new StreamWriter(fs);
            foreach (var item in items)
            {
                string formatted = string.Join(DataSeparator.ToString(), playlistItemToString(item));
                await writer.WriteLineAsync(formatted);
            }

            writer.Flush();
            writer.Close();
        }

        private string[] playlistItemToString(PlaylistItem item)
        {
            return new string[]
            {
                item.FilePath,
                item.LeftPosition.ToString(),
                item.RightPosition.ToString(),
                item.TotalDuration.ToString()
            };
        }
    }
}