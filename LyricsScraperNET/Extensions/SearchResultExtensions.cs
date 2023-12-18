using LyricsScraperNET.Models.Responses;

namespace LyricsScraperNET.Extensions
{
    internal static class SearchResultExtensions
    {
        internal static SearchResult AddErrorMessage(this SearchResult searchResult, string responseMessage)
        {
            searchResult.ResponseStatusCode = ResponseStatusCode.Error;
            return searchResult.AppendResponseMessage(responseMessage);
        }

        internal static SearchResult AddNoDataFoundMessage(this SearchResult searchResult, string responseMessage)
        {
            searchResult.ResponseStatusCode = ResponseStatusCode.NoDataFound;
            return searchResult.AppendResponseMessage(responseMessage);
        }

        internal static SearchResult AddBadRequestMessage(this SearchResult searchResult, string responseMessage)
        {
            searchResult.ResponseStatusCode = ResponseStatusCode.BadRequest;
            return searchResult.AppendResponseMessage(responseMessage);
        }

        internal static SearchResult AppendResponseMessage(this SearchResult searchResult, string responseMessage)
        {
            searchResult.ResponseMessage = !string.IsNullOrWhiteSpace(searchResult.ResponseMessage)
                ? string.Join("; ", new[] { searchResult.ResponseMessage, responseMessage })
                : responseMessage;
            return searchResult;
        }

        internal static SearchResult AddInstrumental(this SearchResult searchResult, bool instrumental)
        {
            searchResult.ResponseStatusCode = ResponseStatusCode.Success;
            searchResult.Instrumental = instrumental;
            return searchResult;
        }
    }
}
