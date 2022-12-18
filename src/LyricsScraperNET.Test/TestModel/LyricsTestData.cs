using System.IO;

namespace LyricsScraper.Test.TestModel
{
    public class LyricsTestData
    {
        public string LyricPagePath { get; set; }
        public string LyricResultPath { get; set; }
        public string ArtistName { get; set; }
        public string SongName { get; set; }
        public string SongUri { get; set; }

        public string LyricPageData => File.ReadAllText(LyricPagePath);

        public string LyricResultData => File.ReadAllText(LyricResultPath);
    }
}
