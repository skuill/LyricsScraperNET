using LyricsScraperNET.Providers.Models;

namespace LyricsScraperNET.Models.Responses
{
    public class SearchResult
    {
        public SearchResult() 
        {
            LyricText = string.Empty;
            ExternalProviderType = ExternalProviderType.None;
        }

        public SearchResult(string lyricText, ExternalProviderType externalProviderType) 
        {
            LyricText = lyricText;
            ExternalProviderType = externalProviderType;
        }

        public string LyricText { get; }

        public ExternalProviderType ExternalProviderType { get; }

        public bool IsEmpty() => string.IsNullOrWhiteSpace(LyricText);
    }
}
