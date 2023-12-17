using System.IO;

namespace LyricsScraperNET.UnitTest.TestModel
{
    public class LyricsTestData
    {
        public string LyricPagePath { get; set; }
        public string LyricResultPath { get; set; }
        public string ArtistName { get; set; }
        public string SongName { get; set; }
        public string SongUri { get; set; }

        public string LyricPageData => ReadFileData(LyricPagePath);

        public string LyricResultData => ReadFileData(LyricResultPath);

        private string ReadFileData(string path) =>
            !string.IsNullOrEmpty(path) ? File.ReadAllText(path) : string.Empty;
    }
}
