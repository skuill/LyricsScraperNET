using LyricsScraperNET.Models.Responses;

namespace LyricsScraperNET.Extensions
{
    internal static class SearchResultExtensions
    {
        internal static SearchResult WithCode(this SearchResult searchResult, SearchResultCode code, string responseMessage)
        {
            searchResult.Code = code;
            return searchResult.AppendResponseMessage(responseMessage);
        }

        internal static SearchResult AppendResponseMessage(this SearchResult searchResult, string responseMessage)
        {
            searchResult.ResponseMessage = !string.IsNullOrWhiteSpace(searchResult.ResponseMessage)
                ? string.Join("; ", new[] { searchResult.ResponseMessage, responseMessage })
                : responseMessage;
            return searchResult;
        }

        internal static SearchResult AsInstrumental(this SearchResult searchResult)
        {
            searchResult.Code = SearchResultCode.Instrumental;
            return searchResult;
        }
    }
}
