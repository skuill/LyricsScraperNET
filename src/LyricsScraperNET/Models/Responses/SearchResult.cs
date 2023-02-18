using LyricsScraperNET.Providers.Models;

namespace LyricsScraperNET.Models.Responses
{
    public class SearchResult
    {
        public SearchResult() 
        { }

        public SearchResult(string lyricText, ExternalProviderType externalProviderType) 
        {
            LyricText = lyricText;
            ExternalProviderType = externalProviderType;
        }

        public string LyricText { get; init; } = string.Empty;

        public ExternalProviderType ExternalProviderType { get; init; } = ExternalProviderType.None;

        public bool IsEmpty() => string.IsNullOrWhiteSpace(LyricText);
    }
}
