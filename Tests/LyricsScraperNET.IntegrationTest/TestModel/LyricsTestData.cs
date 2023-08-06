using System.IO;
using System.Text;

namespace LyricsScraperNET.UnitTest.TestModel
{
    public class LyricsTestData
    {
        public string LyricResultPath { get; set; }
        public string ArtistName { get; set; }
        public string SongName { get; set; }
        public string SongUri { get; set; }

        public string LyricResultData => File.ReadAllText(LyricResultPath);
    }
}
