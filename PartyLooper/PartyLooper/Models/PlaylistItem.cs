using System;
using System.Collections.Generic;
using System.Text;

namespace PartyLooper.Models
{
    public class PlaylistItem
    {
        public string SongName { get; set; }
        public string FilePath { get; set; }
        public double LeftPosition { get; set; }
        public double RightPosition { get; set; }

        public double TotalDuration { get; set; }

        public PlaylistItem()
        {
            LeftPosition = -1;
            RightPosition = -1;
            TotalDuration = 0;
        }
    }
}
